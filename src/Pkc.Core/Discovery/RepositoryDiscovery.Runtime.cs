using System.Text.RegularExpressions;

namespace Pkc.Core.Discovery;

/// <summary>
/// RD4 bounded Tier-2 composition probe. Reads only the own C# files of production hosts and test projects,
/// line by line, looking for reflection assembly-load calls. It extracts a string-literal assembly identity when
/// one is present and never interprets anything else in the source.
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
                    foreach (var line in File.ReadLines(file.FullName))
                    {
                        var code = line.TrimStart();
                        if (code.StartsWith("//", StringComparison.Ordinal))
                        {
                            continue;
                        }

                        foreach (Match match in LoaderCall.Matches(code))
                        {
                            findings.Add(new RuntimeLoaderFinding(
                                project.Manifest,
                                source,
                                LoaderIdentity(match.Groups["api"].Value, match.Groups["rest"].Value)));
                        }
                    }
                }

                probes.Add(new CompositionProbe("dotnet:" + project.Manifest, probed));
            }

            return (findings.Distinct().ToArray(), probes);
        }

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
