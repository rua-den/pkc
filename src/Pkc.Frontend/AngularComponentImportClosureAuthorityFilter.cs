using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Frontend;

internal sealed class AngularComponentImportClosureAuthorityFilter
{
    private const string DefaultExport = "<default>";

    private static readonly Regex ImportRegex = new(
        @"\bimport\s+(?<clause>[^;]+?)\s+from\s*[""'](?<module>[^""']+)[""']",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ReExportRegex = new(
        @"\bexport\s+(?<clause>\{[^}]*\}|\*)\s+from\s*[""'](?<module>[^""']+)[""']",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ComponentDecoratorRegex = new(
        @"@Component\s*\((?<metadata>[\s\S]*?)\)\s*(?:export\s+)?(?:default\s+)?(?:abstract\s+)?class\s+(?<component>[A-Z][A-Za-z0-9_$]*Component)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ImportsPropertyRegex = new(
        @"\bimports\s*:\s*(?<imports>\[[\s\S]*?\]|[A-Za-z_$][A-Za-z0-9_$]*)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ConstantArrayRegex = new(
        @"\b(?:export\s+)?(?:const|let|var)\s+(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\s*=\s*\[(?<body>[\s\S]*?)\]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex DefaultExportRegex = new(
        @"\bexport\s+default\s+(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex DefaultArrayExportRegex = new(
        @"\bexport\s+default\s*\[(?<body>[\s\S]*?)\]",
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
        var sources = await ReadProductSourcesAsync(root, cancellationToken);
        if (sources.Count == 0)
        {
            return document;
        }

        var riskByFile = BuildProjectionRisk(root, sources);
        var blocked = FindBlockedComponents(sources, riskByFile);
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
            var relativePath = Path.GetRelativePath(root, fullPath).Replace('\\', '/');
            sources[fullPath] = new SourceFile(
                fullPath,
                relativePath,
                await File.ReadAllTextAsync(fullPath, cancellationToken));
        }

        return sources;
    }

    private static Dictionary<string, HashSet<string>> BuildProjectionRisk(
        string root,
        IReadOnlyDictionary<string, SourceFile> sources)
    {
        var riskByFile = sources.Keys.ToDictionary(
            file => file,
            _ => new HashSet<string>(StringComparer.Ordinal),
            StringComparer.OrdinalIgnoreCase);
        var bindingsByFile = sources.Keys.ToDictionary(
            file => file,
            _ => new List<LocalBinding>(),
            StringComparer.OrdinalIgnoreCase);
        var exportAllByFile = sources.Keys.ToDictionary(
            file => file,
            _ => new List<string>(),
            StringComparer.OrdinalIgnoreCase);

        foreach (var source in sources.Values)
        {
            var risk = riskByFile[source.FullPath];
            foreach (Match imported in ImportRegex.Matches(source.Text))
            {
                var module = imported.Groups["module"].Value;
                if (IsLocalModule(module))
                {
                    var target = ResolveLocalModule(source.FullPath, module, sources);
                    if (target is null)
                    {
                        continue;
                    }

                    foreach (var binding in ExtractImportBindings(imported.Groups["clause"].Value))
                    {
                        bindingsByFile[source.FullPath].Add(binding with { TargetFile = target });
                    }

                    continue;
                }

                var package = GetPackageName(module);
                if (KnownNonProjectionFrameworkPackages.Contains(package) ||
                    CanResolvePackage(root, source.FullPath, package))
                {
                    continue;
                }

                foreach (var binding in ExtractImportBindings(imported.Groups["clause"].Value))
                {
                    risk.Add(binding.LocalName);
                }
            }

            foreach (Match reExported in ReExportRegex.Matches(source.Text))
            {
                var module = reExported.Groups["module"].Value;
                if (!IsLocalModule(module))
                {
                    continue;
                }

                var target = ResolveLocalModule(source.FullPath, module, sources);
                if (target is null)
                {
                    continue;
                }

                var clause = reExported.Groups["clause"].Value.Trim();
                if (clause == "*")
                {
                    exportAllByFile[source.FullPath].Add(target);
                    continue;
                }

                foreach (var binding in ExtractNamedBindings(clause))
                {
                    bindingsByFile[source.FullPath].Add(binding with { TargetFile = target });
                }
            }

            ExpandArrayRisk(source.Text, risk);
        }

        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var source in sources.Values)
            {
                var risk = riskByFile[source.FullPath];

                foreach (var binding in bindingsByFile[source.FullPath])
                {
                    var targetRisk = riskByFile[binding.TargetFile];
                    var targetText = sources[binding.TargetFile].Text;
                    var isRisky = binding.ImportedName switch
                    {
                        "*" => targetRisk.Count > 0,
                        DefaultExport => IsDefaultExportRisky(targetText, targetRisk),
                        _ => targetRisk.Contains(binding.ImportedName)
                    };

                    if (isRisky && risk.Add(binding.LocalName))
                    {
                        changed = true;
                    }
                }

                foreach (var target in exportAllByFile[source.FullPath])
                {
                    foreach (var identifier in riskByFile[target])
                    {
                        if (risk.Add(identifier))
                        {
                            changed = true;
                        }
                    }
                }

                if (ExpandArrayRisk(source.Text, risk))
                {
                    changed = true;
                }
            }
        }

        return riskByFile;
    }

    private static Dictionary<string, HashSet<string>> FindBlockedComponents(
        IReadOnlyDictionary<string, SourceFile> sources,
        IReadOnlyDictionary<string, HashSet<string>> riskByFile)
    {
        var blocked = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var source in sources.Values)
        {
            var risk = riskByFile[source.FullPath];
            if (risk.Count == 0)
            {
                continue;
            }

            foreach (Match component in ComponentDecoratorRegex.Matches(source.Text))
            {
                var imports = ImportsPropertyRegex.Match(component.Groups["metadata"].Value);
                if (!imports.Success)
                {
                    continue;
                }

                var importedIdentifiers = ResolveComponentImportIdentifiers(
                    source.Text,
                    imports.Groups["imports"].Value);
                if (!risk.Overlaps(importedIdentifiers))
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

    private static bool ExpandArrayRisk(string text, HashSet<string> risk)
    {
        var changedAny = false;
        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (Match array in ConstantArrayRegex.Matches(text))
            {
                var identifiers = IdentifierRegex.Matches(array.Groups["body"].Value)
                    .Select(match => match.Value);
                if (!identifiers.Any(risk.Contains) ||
                    !risk.Add(array.Groups["name"].Value))
                {
                    continue;
                }

                changed = true;
                changedAny = true;
            }
        }

        return changedAny;
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

    private static bool IsDefaultExportRisky(
        string text,
        IReadOnlySet<string> risk)
    {
        var named = DefaultExportRegex.Match(text);
        if (named.Success && risk.Contains(named.Groups["name"].Value))
        {
            return true;
        }

        var array = DefaultArrayExportRegex.Match(text);
        return array.Success &&
               IdentifierRegex.Matches(array.Groups["body"].Value)
                   .Select(match => match.Value)
                   .Any(risk.Contains);
    }

    private static IEnumerable<LocalBinding> ExtractImportBindings(string clause)
    {
        var candidate = clause.Trim();
        if (candidate.StartsWith("type ", StringComparison.Ordinal))
        {
            candidate = candidate[5..].TrimStart();
        }

        var namespaceImport = NamespaceImportRegex.Match(candidate);
        if (namespaceImport.Success)
        {
            yield return new LocalBinding(string.Empty, "*", namespaceImport.Groups["local"].Value);
        }

        foreach (var binding in ExtractNamedBindings(candidate))
        {
            yield return binding;
        }

        var defaultCandidate = candidate.Split(',', 2)[0].Trim();
        if (!defaultCandidate.StartsWith("{", StringComparison.Ordinal) &&
            !defaultCandidate.StartsWith("*", StringComparison.Ordinal) &&
            IsIdentifier(defaultCandidate))
        {
            yield return new LocalBinding(string.Empty, DefaultExport, defaultCandidate);
        }
    }

    private static IEnumerable<LocalBinding> ExtractNamedBindings(string clause)
    {
        var openBrace = clause.IndexOf('{');
        var closeBrace = clause.LastIndexOf('}');
        if (openBrace < 0 || closeBrace <= openBrace)
        {
            yield break;
        }

        var named = clause[(openBrace + 1)..closeBrace];
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
            var importedName = aliasIndex >= 0
                ? entry[..aliasIndex].Trim()
                : entry;
            var localName = aliasIndex >= 0
                ? entry[(aliasIndex + 4)..].Trim()
                : entry;
            if (IsIdentifier(importedName) && IsIdentifier(localName))
            {
                yield return new LocalBinding(string.Empty, importedName, localName);
            }
        }
    }

    private static bool IsIdentifier(string candidate) =>
        IdentifierRegex.Match(candidate) is { Success: true } match &&
        match.Index == 0 &&
        match.Length == candidate.Length;

    private static bool IsLocalModule(string module) =>
        module.StartsWith(".", StringComparison.Ordinal) ||
        module.StartsWith("/", StringComparison.Ordinal);

    private static string? ResolveLocalModule(
        string sourceFile,
        string module,
        IReadOnlyDictionary<string, SourceFile> sources)
    {
        if (!module.StartsWith(".", StringComparison.Ordinal))
        {
            return null;
        }

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

    private sealed record SourceFile(
        string FullPath,
        string RelativePath,
        string Text);

    private sealed record LocalBinding(
        string TargetFile,
        string ImportedName,
        string LocalName);
}
