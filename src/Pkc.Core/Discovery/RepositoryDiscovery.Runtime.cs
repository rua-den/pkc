using System.Text.RegularExpressions;

namespace Pkc.Core.Discovery;

/// <summary>
/// RD4 bounded Tier-2 composition probe. Reads only the own C# files of production hosts and test projects,
/// line by line, looking for reflection assembly-load calls. It extracts a string-literal assembly identity when
/// one is present, or composes a file-scoped subdirectory folder scan, and never interprets anything else.
/// </summary>
public sealed partial class RepositoryDiscovery
{
    private sealed partial class DiscoveryWalk
    {
        private const long MaxProbedFileBytes = 1024 * 1024;

        private static readonly HashSet<ComponentKind> ProbedKinds =
        [
            ComponentKind.WebHost,
            ComponentKind.WorkerHost,
            ComponentKind.ExecutableHost,
            ComponentKind.TestProject
        ];

        private static readonly Regex LoaderCall = new(
            @"(?:\bAssembly\s*\.\s*(?<api>LoadFrom|LoadFile|UnsafeLoadFrom|Load)|\.\s*(?<api>LoadFromAssemblyPath|LoadFromAssemblyName))\s*\((?<rest>.*)$",
            RegexOptions.CultureInvariant);

        private static readonly Regex StringLiteral = new(
            @"@""(?<verbatim>(?:[^""]|"""")*)""|""(?<regular>(?:[^""\\]|\\.)*)""",
            RegexOptions.CultureInvariant);

        private static readonly Regex AssemblyIdentity = new(
            @"^[A-Za-z_][A-Za-z0-9_.]*$",
            RegexOptions.CultureInvariant);

        private (IReadOnlyList<RuntimeLoaderFinding> Loaders, IReadOnlyList<CompositionProbe> Probes) ScanRuntimeLoaders(
            IReadOnlyList<string> files)
        {
            var projectDirectories = _dotnetProjects.Select(project => project.AreaPath).ToHashSet(StringComparer.Ordinal);
            var findings = new List<RuntimeLoaderFinding>();
            var probes = new List<CompositionProbe>();

            foreach (var project in _dotnetProjects
                         .Where(project => ProbedKinds.Contains(project.Kind))
                         .OrderBy(project => project.Manifest, StringComparer.Ordinal))
            {
                var probed = 0;
                foreach (var source in files.Where(file =>
                             file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                             DiscoveryPaths.IsUnder(file, project.AreaPath) &&
                             OwningProjectDirectory(file, projectDirectories) == project.AreaPath &&
                             !IsInsideExcludedArea(file)))
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    var file = RepositoryFile(source);
                    if (file.Length > MaxProbedFileBytes)
                    {
                        continue;
                    }

                    probed++;
                    var lines = File.ReadLines(file.FullName)
                        .Select(line => line.TrimStart())
                        .Where(line => !line.StartsWith("//", StringComparison.Ordinal))
                        .ToArray();
                    findings.AddRange(ScanFile(project.Manifest, source, lines));
                }

                probes.Add(new CompositionProbe("dotnet:" + project.Manifest, probed));
            }

            return (findings.Distinct().ToArray(), probes);
        }

        private static readonly Regex RuntimeBaseDirectory = new(
            @"Path\s*\.\s*Combine\(\s*(?:AppContext\s*\.\s*BaseDirectory|AppDomain\s*\.\s*CurrentDomain\s*\.\s*BaseDirectory)\s*,\s*(?<segment>[^,)]+?)\s*\)",
            RegexOptions.CultureInvariant);

        private static readonly Regex DirectoryEnumeration = new(
            @"\bDirectory\s*\.\s*(?:GetDirectories|EnumerateDirectories)\s*\(",
            RegexOptions.CultureInvariant);

        // A projection lambda over the enumerated directories: dir => Path.Combine(dir, $"{Path.GetFileName(dir)}.dll").
        private static readonly Regex SubdirectoryNamedAssembly = new(
            @"\b(?<dir>[A-Za-z_]\w*)\s*=>[^;]*?Path\s*\.\s*Combine\(\s*\k<dir>\s*,\s*\$""\{Path\s*\.\s*GetFileName\(\s*\k<dir>\s*\)\}\.dll""\s*\)",
            RegexOptions.CultureInvariant);

        private static readonly Regex PathLoadMethodGroup = new(
            @"\bAssembly\s*\.\s*(?:LoadFrom|LoadFile|UnsafeLoadFrom)\b(?!\s*\()",
            RegexOptions.CultureInvariant);

        /// <summary>
        /// RD8-C folder-scan composition, scoped to one file: exactly one literal subdirectory of the runtime base,
        /// a subdirectory enumeration, the <c>&lt;dir&gt;/&lt;dir-name&gt;.dll</c> projection, and a path load that
        /// consumes it (a call whose own arguments contain the projection, or a method group after it). The
        /// composition is file-scoped, not data flow, because the loader may span members. Anything short of it
        /// leaves the loader identity unresolved.
        /// </summary>
        private static IEnumerable<RuntimeLoaderFinding> ScanFile(string manifest, string source, IReadOnlyList<string> lines)
        {
            var masked = lines.Select(line => StringLiteral.Replace(line, "\"\"")).ToArray();
            var segments = lines
                .SelectMany(line => RuntimeBaseDirectory.Matches(line).Select(match => match.Groups["segment"].Value))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var scanDirectory = segments.Length == 1 &&
                                StringLiteral.Match(segments[0]) is { Success: true, Groups: var groups } literal &&
                                literal.Value == segments[0] &&
                                groups["regular"].Success &&
                                IsLiteralScanDirectory(groups["regular"].Value)
                ? groups["regular"].Value.Replace('\\', '/').Trim('/')
                : null;
            var enumerates = masked.Any(line => DirectoryEnumeration.IsMatch(line));
            var firstProjection = Enumerable.Range(0, lines.Count)
                .Where(index => SubdirectoryNamedAssembly.IsMatch(lines[index]))
                .DefaultIfEmpty(-1)
                .First();
            var composable = scanDirectory is not null && enumerates && firstProjection >= 0;

            for (var index = 0; index < lines.Count; index++)
            {
                foreach (Match match in LoaderCall.Matches(lines[index]))
                {
                    var api = match.Groups["api"].Value;
                    var composed = composable &&
                                   api is "LoadFrom" or "LoadFile" or "UnsafeLoadFrom" &&
                                   ProjectionFeedsCall(lines[index], match);
                    yield return composed
                        ? new RuntimeLoaderFinding(manifest, source, null, scanDirectory, "subdirectory-named-assembly")
                        : new RuntimeLoaderFinding(manifest, source, LoaderIdentity(api, match.Groups["rest"].Value));
                }

                foreach (Match _ in PathLoadMethodGroup.Matches(masked[index]))
                {
                    yield return composable && firstProjection <= index
                        ? new RuntimeLoaderFinding(manifest, source, null, scanDirectory, "subdirectory-named-assembly")
                        : new RuntimeLoaderFinding(manifest, source, null);
                }
            }
        }

        /// <summary>The projection must sit inside the load call's own argument list, not merely on the same line.</summary>
        private static bool ProjectionFeedsCall(string line, Match call)
        {
            var arguments = CallArguments(call.Groups["rest"].Value);
            var projection = Regex.Match(arguments,
                @"Path\s*\.\s*Combine\(\s*(?<dir>[A-Za-z_]\w*)\s*,\s*\$""\{Path\s*\.\s*GetFileName\(\s*\k<dir>\s*\)\}\.dll""\s*\)",
                RegexOptions.CultureInvariant);
            return projection.Success &&
                   Regex.IsMatch(line[..call.Index], $@"\b{Regex.Escape(projection.Groups["dir"].Value)}\s*=>", RegexOptions.CultureInvariant);
        }

        private static bool IsLiteralScanDirectory(string value) =>
            !string.IsNullOrWhiteSpace(value) && !value.Contains('$') && !value.Contains("..", StringComparison.Ordinal) &&
            !Path.IsPathRooted(value) && value.Split('/', '\\').All(segment => segment.Length > 0);

        private static string? OwningProjectDirectory(string file, IReadOnlySet<string> projectDirectories) =>
            DiscoveryPaths.SelfAndAncestors(DiscoveryPaths.Parent(file)).FirstOrDefault(projectDirectories.Contains);

        /// <summary>
        /// Only a string literal inside the call's own argument list is identity evidence:
        /// an assembly name for Load/LoadFromAssemblyName, a `*.dll` file name for path-based loads.
        /// </summary>
        private static string? LoaderIdentity(string api, string rest)
        {
            var arguments = CallArguments(rest);
            var literals = StringLiteral.Matches(arguments)
                .Select(literal => literal.Groups["verbatim"].Success
                    ? literal.Groups["verbatim"].Value.Replace("\"\"", "\"")
                    : literal.Groups["regular"].Value.Replace(@"\\", @"\"))
                .ToArray();

            string? identity;
            if (api is "Load" or "LoadFromAssemblyName")
            {
                identity = literals.FirstOrDefault()?.Split(',')[0].Trim();
            }
            else
            {
                var dll = literals.LastOrDefault(literal => literal.EndsWith(".dll", StringComparison.OrdinalIgnoreCase));
                identity = dll is null ? null : Path.GetFileNameWithoutExtension(dll.Replace('\\', '/').Split('/').Last());
            }

            return identity is not null && AssemblyIdentity.IsMatch(identity) ? identity : null;
        }

        /// <summary>Text up to the parenthesis that closes the call, ignoring parentheses inside string literals.</summary>
        private static string CallArguments(string rest)
        {
            var depth = 1;
            var inString = false;
            for (var index = 0; index < rest.Length; index++)
            {
                var character = rest[index];
                if (character == '"' && (index == 0 || rest[index - 1] != '\\'))
                {
                    inString = !inString;
                }
                else if (!inString && character == '(')
                {
                    depth++;
                }
                else if (!inString && character == ')' && --depth == 0)
                {
                    return rest[..index];
                }
            }

            return rest;
        }
    }
}
