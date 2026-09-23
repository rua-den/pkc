using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Frontend;

internal sealed class AngularUnresolvedExternalComponentImportAuthorityFilter
{
    private static readonly Regex ExternalImportRegex = new(
        @"\bimport\s+(?<clause>[^;]+?)\s+from\s*[""'](?<module>[^""']+)[""']",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ComponentDecoratorRegex = new(
        @"@Component\s*\((?<metadata>[\s\S]*?)\)\s*(?:export\s+)?(?:default\s+)?(?:abstract\s+)?class\s+(?<component>[A-Z][A-Za-z0-9_$]*Component)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ImportsPropertyRegex = new(
        @"\bimports\s*:\s*(?<imports>\[[\s\S]*?\]|[A-Za-z_$][A-Za-z0-9_$]*)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex IdentifierRegex = new(
        @"[A-Za-z_$][A-Za-z0-9_$]*",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex NamespaceImportRegex = new(
        @"\*\s+as\s+(?<local>[A-Za-z_$][A-Za-z0-9_$]*)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex PackageNameRegex = new(
        @"^(?:@[A-Za-z0-9._~-]+/[A-Za-z0-9._~-]+|[A-Za-z0-9._~-]+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> KnownNonProjectionFrameworkPackages = new(
        StringComparer.Ordinal)
    {
        "@angular/core",
        "@angular/common",
        "@angular/forms",
        "@angular/router",
        "@angular/animations",
        "@angular/platform-browser",
        "@angular/platform-browser-dynamic",
        "@angular/service-worker",
        "@angular/localize"
    };

    public async Task<FactDocument> FilterAsync(
        string repositoryPath,
        FactDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        ArgumentNullException.ThrowIfNull(document);

        var root = Path.GetFullPath(repositoryPath);
        var blocked = await FindBlockedComponentsAsync(root, cancellationToken);
        if (blocked.Count == 0)
        {
            return document;
        }

        var rejected = document.Facts
            .Where(fact =>
                fact.Kind == "ui-member-render" &&
                fact.Metadata.ContainsKey("renderAuthority") &&
                fact.Metadata.TryGetValue("component", out var component) &&
                blocked.TryGetValue(fact.Source.Path, out var components) &&
                components.Contains(component))
            .Select(fact => fact.Id)
            .ToHashSet(StringComparer.Ordinal);

        if (rejected.Count == 0)
        {
            return document;
        }

        return document with
        {
            Facts = document.Facts
                .Where(fact => !rejected.Contains(fact.Id))
                .ToArray(),
            Relations = document.Relations
                .Where(relation =>
                    !rejected.Contains(relation.FromFactId) &&
                    !rejected.Contains(relation.Target))
                .ToArray()
        };
    }

    private static async Task<Dictionary<string, HashSet<string>>> FindBlockedComponentsAsync(
        string root,
        CancellationToken cancellationToken)
    {
        var blocked = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var file in EnumerateProductTypeScriptFiles(root, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var text = await File.ReadAllTextAsync(file, cancellationToken);
            var unresolvedLocals = new HashSet<string>(StringComparer.Ordinal);

            foreach (Match imported in ExternalImportRegex.Matches(text))
            {
                var module = imported.Groups["module"].Value;
                if (module.StartsWith(".", StringComparison.Ordinal) ||
                    module.StartsWith("/", StringComparison.Ordinal))
                {
                    continue;
                }

                var package = GetPackageName(module);
                if (KnownNonProjectionFrameworkPackages.Contains(package) ||
                    CanResolvePackage(root, file, package))
                {
                    continue;
                }

                foreach (var local in ExtractImportedLocalNames(imported.Groups["clause"].Value))
                {
                    unresolvedLocals.Add(local);
                }
            }

            if (unresolvedLocals.Count == 0)
            {
                continue;
            }

            var relativePath = Path.GetRelativePath(root, file).Replace('\\', '/');
            foreach (Match component in ComponentDecoratorRegex.Matches(text))
            {
                var imports = ImportsPropertyRegex.Match(component.Groups["metadata"].Value);
                if (!imports.Success)
                {
                    continue;
                }

                var importedIdentifiers = ResolveComponentImportIdentifiers(
                    text,
                    imports.Groups["imports"].Value);
                if (!unresolvedLocals.Overlaps(importedIdentifiers))
                {
                    continue;
                }

                if (!blocked.TryGetValue(relativePath, out var components))
                {
                    components = new HashSet<string>(StringComparer.Ordinal);
                    blocked[relativePath] = components;
                }

                components.Add(component.Groups["component"].Value);
            }
        }

        return blocked;
    }

    private static HashSet<string> ResolveComponentImportIdentifiers(
        string text,
        string expression)
    {
        var identifiers = new HashSet<string>(StringComparer.Ordinal);
        var expanded = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>();

        AddIdentifiers(expression, identifiers, pending);
        while (pending.Count > 0)
        {
            var identifier = pending.Pop();
            if (!expanded.Add(identifier) ||
                !TryFindConstantArrayBody(text, identifier, out var body))
            {
                continue;
            }

            AddIdentifiers(body, identifiers, pending);
        }

        return identifiers;
    }

    private static void AddIdentifiers(
        string expression,
        HashSet<string> identifiers,
        Stack<string> pending)
    {
        foreach (Match match in IdentifierRegex.Matches(expression))
        {
            if (identifiers.Add(match.Value))
            {
                pending.Push(match.Value);
            }
        }
    }

    private static bool TryFindConstantArrayBody(
        string text,
        string identifier,
        out string body)
    {
        var pattern = @"\b(?:const|let|var)\s+" + Regex.Escape(identifier) +
                      @"\s*=\s*\[(?<body>[\s\S]*?)\]";
        var match = Regex.Match(text, pattern, RegexOptions.CultureInvariant);
        body = match.Success ? match.Groups["body"].Value : string.Empty;
        return match.Success;
    }

    private static IEnumerable<string> ExtractImportedLocalNames(string clause)
    {
        var candidate = clause.Trim();
        if (candidate.StartsWith("type ", StringComparison.Ordinal))
        {
            candidate = candidate[5..].TrimStart();
        }

        var namespaceImport = NamespaceImportRegex.Match(candidate);
        if (namespaceImport.Success)
        {
            yield return namespaceImport.Groups["local"].Value;
        }

        var openBrace = candidate.IndexOf('{');
        var closeBrace = candidate.LastIndexOf('}');
        if (openBrace >= 0 && closeBrace > openBrace)
        {
            var named = candidate[(openBrace + 1)..closeBrace];
            foreach (var rawEntry in named.Split(','))
            {
                var entry = rawEntry.Trim();
                if (entry.StartsWith("type ", StringComparison.Ordinal))
                {
                    entry = entry[5..].TrimStart();
                }

                if (entry.Length == 0)
                {
                    continue;
                }

                var aliasIndex = entry.IndexOf(" as ", StringComparison.Ordinal);
                var local = aliasIndex >= 0
                    ? entry[(aliasIndex + 4)..].Trim()
                    : entry;
                if (IdentifierRegex.Match(local) is { Success: true } localMatch &&
                    localMatch.Index == 0 &&
                    localMatch.Length == local.Length)
                {
                    yield return local;
                }
            }
        }

        var defaultCandidate = candidate.Split(',', 2)[0].Trim();
        if (!defaultCandidate.StartsWith("{", StringComparison.Ordinal) &&
            !defaultCandidate.StartsWith("*", StringComparison.Ordinal) &&
            IdentifierRegex.Match(defaultCandidate) is { Success: true } defaultMatch &&
            defaultMatch.Index == 0 &&
            defaultMatch.Length == defaultCandidate.Length)
        {
            yield return defaultCandidate;
        }
    }

    private static bool CanResolvePackage(
        string repositoryRoot,
        string sourceFile,
        string package)
    {
        if (!PackageNameRegex.IsMatch(package))
        {
            return false;
        }

        var root = NormalizeDirectoryPath(repositoryRoot);
        var current = Path.GetDirectoryName(Path.GetFullPath(sourceFile));
        var packagePath = package.Replace('/', Path.DirectorySeparatorChar);
        while (current is not null && IsPathWithinOrEqual(root, current))
        {
            var nodeModulesRoot = Path.GetFullPath(Path.Combine(current, "node_modules"));
            var packageRoot = Path.GetFullPath(Path.Combine(nodeModulesRoot, packagePath));
            var containmentPrefix = NormalizeDirectoryPath(nodeModulesRoot) + Path.DirectorySeparatorChar;
            if (packageRoot.StartsWith(containmentPrefix, StringComparison.OrdinalIgnoreCase) &&
                Directory.Exists(packageRoot))
            {
                var packageDirectory = new DirectoryInfo(packageRoot);
                FileSystemInfo? resolved = packageDirectory;
                if ((packageDirectory.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    try
                    {
                        resolved = packageDirectory.ResolveLinkTarget(returnFinalTarget: true);
                    }
                    catch (IOException)
                    {
                        return false;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return false;
                    }
                    catch (NotSupportedException)
                    {
                        return false;
                    }
                }

                return resolved is not null &&
                       resolved.Exists &&
                       IsPathWithinOrEqual(root, resolved.FullName);
            }

            if (string.Equals(
                    NormalizeDirectoryPath(current),
                    root,
                    StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            current = Path.GetDirectoryName(current);
        }

        return false;
    }

    private static string GetPackageName(string module)
    {
        var segments = module.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return module;
        }

        return segments[0].StartsWith("@", StringComparison.Ordinal) && segments.Length > 1
            ? $"{segments[0]}/{segments[1]}"
            : segments[0];
    }

    private static bool IsPathWithinOrEqual(string root, string candidate)
    {
        var normalizedRoot = NormalizeDirectoryPath(root);
        var normalizedCandidate = NormalizeDirectoryPath(candidate);
        return string.Equals(normalizedRoot, normalizedCandidate, StringComparison.OrdinalIgnoreCase) ||
               normalizedCandidate.StartsWith(
                   normalizedRoot + Path.DirectorySeparatorChar,
                   StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeDirectoryPath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var pathRoot = Path.GetPathRoot(fullPath);
        if (string.Equals(fullPath, pathRoot, StringComparison.OrdinalIgnoreCase))
        {
            return fullPath;
        }

        return fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static IEnumerable<string> EnumerateProductTypeScriptFiles(
        string root,
        CancellationToken cancellationToken)
    {
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var directory = pending.Pop();
            var entries = Directory.EnumerateFileSystemEntries(directory)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var attributes = File.GetAttributes(entry);
                if ((attributes & FileAttributes.Directory) != 0)
                {
                    var name = Path.GetFileName(entry);
                    if (!FrontendSourceScope.IsExcludedDirectoryName(name) &&
                        (attributes & FileAttributes.ReparsePoint) == 0)
                    {
                        pending.Push(entry);
                    }
                }
                else if (entry.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
                {
                    var relativePath = Path.GetRelativePath(root, entry).Replace('\\', '/');
                    if (FrontendSourceScope.IsProductSource(relativePath))
                    {
                        yield return entry;
                    }
                }
            }
        }
    }
}
