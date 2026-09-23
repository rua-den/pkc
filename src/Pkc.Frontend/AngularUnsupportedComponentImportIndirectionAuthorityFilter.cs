using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Frontend;

internal sealed class AngularUnsupportedComponentImportIndirectionAuthorityFilter
{
    private static readonly Regex ComponentDecoratorRegex = new(
        @"@Component\s*\((?<metadata>[\s\S]*?)\)\s*(?:export\s+)?(?:default\s+)?(?:abstract\s+)?class\s+(?<component>[A-Z][A-Za-z0-9_$]*Component)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ImportsPropertyRegex = new(
        @"\bimports\s*:\s*(?<imports>\[[\s\S]*?\]|[A-Za-z_$][A-Za-z0-9_$]*)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex IdentifierRegex = new(
        @"[A-Za-z_$][A-Za-z0-9_$]*",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ScalarAliasRegex = new(
        @"\b(?:const|let|var)\s+(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\s*=\s*(?<target>[A-Za-z_$][A-Za-z0-9_$]*)\s*;",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex LocalImportRegex = new(
        @"\bimport\s+(?<clause>[^;]+?)\s+from\s*[""'](?<module>\.[^""']*)[""']",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex LocalReExportRegex = new(
        @"\bexport\s+(?<clause>\{[^}]*\}|\*)\s+from\s*[""'](?<module>\.[^""']*)[""']",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public async Task<FactDocument> FilterAsync(
        string repositoryPath,
        FactDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        ArgumentNullException.ThrowIfNull(document);

        var root = Path.GetFullPath(repositoryPath);
        var sources = await ReadProductSourcesAsync(root, cancellationToken);
        if (sources.Count == 0)
        {
            return document;
        }

        var blocked = FindBlockedComponents(sources);
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

    private static async Task<Dictionary<string, SourceFile>> ReadProductSourcesAsync(
        string root,
        CancellationToken cancellationToken)
    {
        var sources = new Dictionary<string, SourceFile>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in EnumerateProductTypeScriptFiles(root, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fullPath = Path.GetFullPath(file);
            sources[fullPath] = new SourceFile(
                fullPath,
                Path.GetRelativePath(root, fullPath).Replace('\\', '/'),
                await File.ReadAllTextAsync(fullPath, cancellationToken));
        }

        return sources;
    }

    private static Dictionary<string, HashSet<string>> FindBlockedComponents(
        IReadOnlyDictionary<string, SourceFile> sources)
    {
        var blocked = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        foreach (var source in sources.Values)
        {
            var aliases = ScalarAliasRegex.Matches(source.Text)
                .Cast<Match>()
                .ToDictionary(
                    match => match.Groups["name"].Value,
                    match => match.Groups["target"].Value,
                    StringComparer.Ordinal);
            var imports = ReadLocalImports(source, sources);

            foreach (Match component in ComponentDecoratorRegex.Matches(source.Text))
            {
                var importsProperty = ImportsPropertyRegex.Match(component.Groups["metadata"].Value);
                if (!importsProperty.Success)
                {
                    continue;
                }

                var identifiers = IdentifierRegex.Matches(importsProperty.Groups["imports"].Value)
                    .Select(match => match.Value)
                    .ToHashSet(StringComparer.Ordinal);

                var unsupportedScalarAlias = identifiers.Any(identifier =>
                    HasScalarAlias(identifier, aliases));
                var unsupportedDefaultReExport = imports.Any(imported =>
                    identifiers.Contains(imported.LocalName) &&
                    HasDefaultReExport(
                        imported.TargetFile,
                        imported.ImportedName,
                        sources,
                        new HashSet<string>(StringComparer.OrdinalIgnoreCase)));

                if (!unsupportedScalarAlias && !unsupportedDefaultReExport)
                {
                    continue;
                }

                if (!blocked.TryGetValue(source.RelativePath, out var components))
                {
                    components = new HashSet<string>(StringComparer.Ordinal);
                    blocked[source.RelativePath] = components;
                }

                components.Add(component.Groups["component"].Value);
            }
        }

        return blocked;
    }

    private static bool HasScalarAlias(
        string identifier,
        IReadOnlyDictionary<string, string> aliases)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var current = identifier;
        while (aliases.TryGetValue(current, out var target))
        {
            if (!visited.Add(current))
            {
                return true;
            }

            current = target;
        }

        return visited.Count > 0;
    }

    private static IReadOnlyList<LocalImport> ReadLocalImports(
        SourceFile source,
        IReadOnlyDictionary<string, SourceFile> sources)
    {
        var imports = new List<LocalImport>();
        foreach (Match match in LocalImportRegex.Matches(source.Text))
        {
            var target = ResolveLocalModule(source.FullPath, match.Groups["module"].Value, sources);
            if (target is null)
            {
                continue;
            }

            var clause = match.Groups["clause"].Value.Trim();
            var openBrace = clause.IndexOf('{');
            var closeBrace = clause.LastIndexOf('}');
            if (openBrace >= 0 && closeBrace > openBrace)
            {
                foreach (var rawEntry in clause[(openBrace + 1)..closeBrace].Split(','))
                {
                    var entry = rawEntry.Trim();
                    if (entry.Length == 0 || entry.StartsWith("type ", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var aliasIndex = entry.IndexOf(" as ", StringComparison.Ordinal);
                    var importedName = aliasIndex >= 0 ? entry[..aliasIndex].Trim() : entry;
                    var localName = aliasIndex >= 0 ? entry[(aliasIndex + 4)..].Trim() : entry;
                    if (IsIdentifier(importedName) && IsIdentifier(localName))
                    {
                        imports.Add(new LocalImport(target, importedName, localName));
                    }
                }
            }
        }

        return imports;
    }

    private static bool HasDefaultReExport(
        string file,
        string symbol,
        IReadOnlyDictionary<string, SourceFile> sources,
        HashSet<string> visited)
    {
        var visitKey = $"{file}|{symbol}";
        if (!visited.Add(visitKey) || !sources.TryGetValue(file, out var source))
        {
            return false;
        }

        foreach (Match match in LocalReExportRegex.Matches(source.Text))
        {
            var target = ResolveLocalModule(source.FullPath, match.Groups["module"].Value, sources);
            if (target is null)
            {
                continue;
            }

            var clause = match.Groups["clause"].Value.Trim();
            if (clause == "*")
            {
                if (HasDefaultReExport(target, symbol, sources, visited))
                {
                    return true;
                }

                continue;
            }

            var named = clause[1..^1];
            foreach (var rawEntry in named.Split(','))
            {
                var entry = rawEntry.Trim();
                if (entry.Length == 0 || entry.StartsWith("type ", StringComparison.Ordinal))
                {
                    continue;
                }

                var aliasIndex = entry.IndexOf(" as ", StringComparison.Ordinal);
                var importedName = aliasIndex >= 0 ? entry[..aliasIndex].Trim() : entry;
                var exportedName = aliasIndex >= 0 ? entry[(aliasIndex + 4)..].Trim() : entry;
                if (!string.Equals(exportedName, symbol, StringComparison.Ordinal))
                {
                    continue;
                }

                if (string.Equals(importedName, "default", StringComparison.Ordinal))
                {
                    return true;
                }

                if (HasDefaultReExport(target, importedName, sources, visited))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsIdentifier(string candidate) =>
        IdentifierRegex.Match(candidate) is { Success: true } match &&
        match.Index == 0 &&
        match.Length == candidate.Length;

    private static string? ResolveLocalModule(
        string sourceFile,
        string module,
        IReadOnlyDictionary<string, SourceFile> sources)
    {
        var directory = Path.GetDirectoryName(sourceFile);
        if (directory is null)
        {
            return null;
        }

        var modulePath = module.Replace('/', Path.DirectorySeparatorChar);
        var basePath = Path.GetFullPath(Path.Combine(directory, modulePath));
        var candidates = new List<string>
        {
            basePath,
            basePath + ".ts",
            Path.Combine(basePath, "index.ts")
        };
        if (basePath.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
        {
            candidates.Add(basePath[..^3] + ".ts");
        }

        foreach (var candidate in candidates)
        {
            var fullPath = Path.GetFullPath(candidate);
            if (sources.ContainsKey(fullPath))
            {
                return fullPath;
            }
        }

        return null;
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
            foreach (var entry in Directory.EnumerateFileSystemEntries(directory)
                         .OrderBy(path => path, StringComparer.Ordinal))
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

    private sealed record SourceFile(string FullPath, string RelativePath, string Text);

    private sealed record LocalImport(string TargetFile, string ImportedName, string LocalName);
}
