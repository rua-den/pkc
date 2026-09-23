using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Frontend;

internal sealed class AngularComponentProjectionAuthorityFilter
{
    private static readonly Regex ComponentTemplateRegex = new(
        @"@Component\s*\(\s*\{(?<before>[\s\S]*?)\btemplate\s*:\s*`(?<body>[\s\S]*?)`(?<after>[\s\S]*?)\}\s*\)\s*(?:export\s+)?class\s+(?<component>[A-Z][A-Za-z0-9_$]*Component)\b",
        RegexOptions.Compiled);

    private static readonly Regex ComponentDecoratorRegex = new(
        @"@Component\s*\((?<metadata>[\s\S]*?)\)\s*(?:export\s+)?(?:default\s+)?(?:abstract\s+)?class\s+[A-Z][A-Za-z0-9_$]*Component\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex SelectorPropertyRegex = new(
        @"\bselector\s*:\s*(?<quote>[""'])(?<selector>[^""']+)\k<quote>",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ImportedModuleRegex = new(
        @"\bfrom\s*[""'](?<module>[^""']+)[""']|\bimport\s*[""'](?<module>[^""']+)[""']",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex PackageNameRegex = new(
        @"^(?:@[A-Za-z0-9._~-]+/[A-Za-z0-9._~-]+|[A-Za-z0-9._~-]+)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex HtmlElementTagRegex = new(
        @"<\s*(?<closing>/)?\s*(?<name>[A-Za-z][A-Za-z0-9:-]*)\b(?<attrs>(?:[^""'<>]|""[^""]*""|'[^']*')*)>",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex SelectorNotRegex = new(
        @":not\([^)]*\)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex SelectorTypeRegex = new(
        @"^[A-Za-z][A-Za-z0-9_-]*",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex SelectorClassRegex = new(
        @"\.(?<name>[A-Za-z_][A-Za-z0-9_-]*)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex SelectorAttributeRegex = new(
        @"\[\s*(?<name>[A-Za-z_:][A-Za-z0-9_:.-]*)(?:\s*=\s*(?<quote>[""']?)(?<value>[^\]""']+)\k<quote>)?\s*\]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex StaticAttributeRegex = new(
        @"(?<name>[^\s=/]+)(?:\s*=\s*(?<quote>[""'])(?<value>.*?)\k<quote>|\s*=\s*(?<bare>[^\s/]+))?",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> VoidHtmlElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr"
    };

    public async Task<FactDocument> FilterAsync(
        string repositoryPath,
        FactDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        ArgumentNullException.ThrowIfNull(document);

        var root = Path.GetFullPath(repositoryPath);
        var componentSelectors = await FindComponentSelectorsAsync(root, cancellationToken);
        var cache = new Dictionary<string, string>(StringComparer.Ordinal);
        var rejected = new HashSet<string>(StringComparer.Ordinal);

        foreach (var fact in document.Facts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (fact.Kind != "ui-member-render" ||
                !fact.Metadata.ContainsKey("renderAuthority"))
            {
                continue;
            }

            if (!fact.Metadata.TryGetValue("component", out var component) ||
                string.IsNullOrWhiteSpace(component) ||
                !fact.Metadata.TryGetValue("member", out var member) ||
                string.IsNullOrWhiteSpace(member))
            {
                rejected.Add(fact.Id);
                continue;
            }

            if (!cache.TryGetValue(fact.Source.Path, out var text))
            {
                var fullPath = Path.Combine(
                    root,
                    fact.Source.Path.Replace('/', Path.DirectorySeparatorChar));
                text = File.Exists(fullPath)
                    ? await File.ReadAllTextAsync(fullPath, cancellationToken)
                    : string.Empty;
                cache[fact.Source.Path] = text;
            }

            if (string.IsNullOrEmpty(text) ||
                !HasSupportedRenderContext(
                    text,
                    component,
                    member,
                    fact.Source.StartLine,
                    componentSelectors))
            {
                rejected.Add(fact.Id);
            }
        }

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

    private static async Task<string[]> FindComponentSelectorsAsync(
        string root,
        CancellationToken cancellationToken)
    {
        var selectors = new HashSet<string>(StringComparer.Ordinal);
        var importedPackageRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in EnumerateProductTypeScriptFiles(root, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var text = await File.ReadAllTextAsync(file, cancellationToken);
            foreach (Match importedModule in ImportedModuleRegex.Matches(text))
            {
                var module = importedModule.Groups["module"].Value;
                if (module.StartsWith(".", StringComparison.Ordinal) ||
                    module.StartsWith("/", StringComparison.Ordinal))
                {
                    continue;
                }

                var package = GetPackageName(module);
                var packageRoot = FindImportedPackageRoot(root, file, package);
                if (packageRoot is not null)
                {
                    importedPackageRoots.Add(packageRoot);
                }
            }

            foreach (Match component in ComponentDecoratorRegex.Matches(text))
            {
                var selector = SelectorPropertyRegex.Match(component.Groups["metadata"].Value);
                if (!selector.Success)
                {
                    continue;
                }

                foreach (var candidate in selector.Groups["selector"].Value.Split(','))
                {
                    var value = candidate.Trim();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        selectors.Add(value);
                    }
                }
            }
        }

        foreach (var packageRoot in importedPackageRoots.OrderBy(value => value, StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var file in EnumerateDeclarationFiles(packageRoot, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var text = await File.ReadAllTextAsync(file, cancellationToken);
                foreach (var selector in ExtractExternalComponentSelectors(text))
                {
                    foreach (var candidate in selector.Split(','))
                    {
                        var value = candidate.Trim();
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            selectors.Add(value);
                        }
                    }
                }
            }
        }

        return selectors.OrderBy(value => value, StringComparer.Ordinal).ToArray();
    }

    private static string? FindImportedPackageRoot(
        string repositoryRoot,
        string sourceFile,
        string package)
    {
        if (!PackageNameRegex.IsMatch(package))
        {
            return null;
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
                return ResolveContainedPackageRoot(nodeModulesRoot, packageRoot);
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

        return null;
    }

    private static string? ResolveContainedPackageRoot(string nodeModulesRoot, string packageRoot)
    {
        var packageDirectory = new DirectoryInfo(packageRoot);
        DirectoryInfo? resolved = packageDirectory;
        if ((packageDirectory.Attributes & FileAttributes.ReparsePoint) != 0)
        {
            try
            {
                resolved = packageDirectory.ResolveLinkTarget(returnFinalTarget: true);
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }
        }

        if (resolved is null || !resolved.Exists)
        {
            return null;
        }

        var resolvedPath = NormalizeDirectoryPath(resolved.FullName);
        return IsPathWithinOrEqual(nodeModulesRoot, resolvedPath)
            ? resolvedPath
            : null;
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

    private static IEnumerable<string> EnumerateDeclarationFiles(
        string packageRoot,
        CancellationToken cancellationToken)
    {
        var pending = new Stack<string>();
        pending.Push(packageRoot);
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
                    if ((attributes & FileAttributes.ReparsePoint) == 0)
                    {
                        pending.Push(entry);
                    }
                }
                else if (entry.EndsWith(".d.ts", StringComparison.OrdinalIgnoreCase))
                {
                    yield return entry;
                }
            }
        }
    }

    private static IEnumerable<string> ExtractExternalComponentSelectors(string text)
    {
        const string marker = "ɵɵComponentDeclaration";
        var searchStart = 0;
        while (searchStart < text.Length)
        {
            var markerIndex = text.IndexOf(marker, searchStart, StringComparison.Ordinal);
            if (markerIndex < 0)
            {
                yield break;
            }

            if (TryParseSecondGenericString(text, markerIndex + marker.Length, out var selector))
            {
                yield return selector;
            }

            searchStart = markerIndex + marker.Length;
        }
    }

    private static bool TryParseSecondGenericString(
        string text,
        int start,
        out string selector)
    {
        selector = string.Empty;
        var open = text.IndexOf('<', start);
        if (open < 0)
        {
            return false;
        }

        var depth = 1;
        var firstComma = -1;
        var index = open + 1;
        while (index < text.Length)
        {
            var character = text[index];
            if (character is '\'' or '"')
            {
                if (!SkipTypeScriptString(text, ref index))
                {
                    return false;
                }

                continue;
            }

            if (character == ';' && depth == 1)
            {
                return false;
            }

            if (character == '<')
            {
                depth++;
            }
            else if (character == '>')
            {
                depth--;
                if (depth == 0)
                {
                    return false;
                }
            }
            else if (character == ',' && depth == 1)
            {
                if (firstComma < 0)
                {
                    firstComma = index;
                }
                else
                {
                    var candidate = text[(firstComma + 1)..index].Trim();
                    return TryParseTypeScriptStringLiteral(candidate, out selector);
                }
            }

            index++;
        }

        return false;
    }

    private static bool SkipTypeScriptString(string text, ref int index)
    {
        var quote = text[index++];
        while (index < text.Length)
        {
            if (text[index] == '\\')
            {
                index += 2;
                continue;
            }

            if (text[index] == quote)
            {
                index++;
                return true;
            }

            index++;
        }

        return false;
    }

    private static bool TryParseTypeScriptStringLiteral(string text, out string value)
    {
        value = string.Empty;
        if (text.Length < 2 || (text[0] != '\'' && text[0] != '"') || text[^1] != text[0])
        {
            return false;
        }

        var builder = new System.Text.StringBuilder(text.Length - 2);
        for (var index = 1; index < text.Length - 1; index++)
        {
            if (text[index] != '\\')
            {
                builder.Append(text[index]);
                continue;
            }

            if (++index >= text.Length - 1)
            {
                return false;
            }

            var escaped = text[index];
            builder.Append(escaped switch
            {
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                'b' => '\b',
                'f' => '\f',
                'v' => '\v',
                '0' => '\0',
                _ => escaped
            });
        }

        value = builder.ToString();
        return true;
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

    private static bool HasSupportedRenderContext(
        string text,
        string component,
        string member,
        int sourceLine,
        IReadOnlyList<string> componentSelectors)
    {
        var matches = 0;
        var supported = 0;

        foreach (Match template in ComponentTemplateRegex.Matches(text))
        {
            if (!string.Equals(
                    template.Groups["component"].Value,
                    component,
                    StringComparison.Ordinal))
            {
                continue;
            }

            var body = template.Groups["body"];
            var interpolationRegex = new Regex(
                @"\{\{\s*" + Regex.Escape(member) + @"\s*\}\}",
                RegexOptions.CultureInvariant);

            foreach (Match interpolation in interpolationRegex.Matches(body.Value))
            {
                var absoluteIndex = body.Index + interpolation.Index;
                var line = 1 + text.AsSpan(0, absoluteIndex).Count('\n');
                if (line != sourceLine)
                {
                    continue;
                }

                matches++;
                if (!IsInsideUnprovenProjectionBoundary(
                        body.Value,
                        interpolation.Index,
                        componentSelectors))
                {
                    supported++;
                }
            }
        }

        return matches == 1 && supported == 1;
    }

    private static bool IsInsideUnprovenProjectionBoundary(
        string templateBody,
        int position,
        IReadOnlyList<string> componentSelectors)
    {
        var ancestors = new Stack<ElementFrame>();
        foreach (Match tag in HtmlElementTagRegex.Matches(templateBody))
        {
            if (tag.Index >= position)
            {
                break;
            }

            if (IsInsideHtmlComment(templateBody, tag.Index))
            {
                continue;
            }

            var name = tag.Groups["name"].Value;
            if (tag.Groups["closing"].Success)
            {
                if (VoidHtmlElements.Contains(name))
                {
                    continue;
                }

                if (ancestors.Count == 0 ||
                    !string.Equals(ancestors.Peek().Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                ancestors.Pop();
                continue;
            }

            var attrs = tag.Groups["attrs"].Value;
            var selfClosing = attrs.TrimEnd().EndsWith("/", StringComparison.Ordinal);
            if (selfClosing || VoidHtmlElements.Contains(name))
            {
                continue;
            }

            ancestors.Push(new ElementFrame(name, attrs));
        }

        return ancestors.Any(frame =>
            IsUnprovenProjectionBoundary(frame, componentSelectors));
    }

    private static bool IsUnprovenProjectionBoundary(
        ElementFrame frame,
        IReadOnlyList<string> componentSelectors)
    {
        if (string.Equals(frame.Name, "ng-container", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (frame.Name.Contains("-", StringComparison.Ordinal))
        {
            return true;
        }

        return componentSelectors.Any(selector => CouldMatchSelector(frame, selector));
    }

    private static bool CouldMatchSelector(ElementFrame frame, string selector)
    {
        var candidate = SelectorNotRegex.Replace(selector, string.Empty).Trim();
        if (candidate.Length == 0)
        {
            return false;
        }

        var matchedConstraint = false;
        var type = SelectorTypeRegex.Match(candidate);
        if (type.Success)
        {
            matchedConstraint = true;
            if (!string.Equals(type.Value, frame.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        var attributes = ParseStaticAttributes(frame.Attributes);
        foreach (Match classMatch in SelectorClassRegex.Matches(candidate))
        {
            matchedConstraint = true;
            if (!attributes.TryGetValue("class", out var classValue) ||
                classValue is null ||
                !classValue.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                    .Contains(classMatch.Groups["name"].Value, StringComparer.Ordinal))
            {
                return false;
            }
        }

        foreach (Match attributeMatch in SelectorAttributeRegex.Matches(candidate))
        {
            matchedConstraint = true;
            var name = attributeMatch.Groups["name"].Value;
            if (!attributes.TryGetValue(name, out var actualValue))
            {
                return false;
            }

            if (!attributeMatch.Groups["value"].Success)
            {
                continue;
            }

            var expectedValue = attributeMatch.Groups["value"].Value.Trim();
            if (actualValue is null ||
                !string.Equals(actualValue, expectedValue, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return matchedConstraint;
    }

    private static Dictionary<string, string?> ParseStaticAttributes(string attributes)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in StaticAttributeRegex.Matches(attributes))
        {
            var rawName = match.Groups["name"].Value;
            if (string.IsNullOrWhiteSpace(rawName) || rawName == "/")
            {
                continue;
            }

            var normalizedName = NormalizeBoundAttributeName(rawName);
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                continue;
            }

            string? value = null;
            if (match.Groups["value"].Success)
            {
                value = match.Groups["value"].Value;
            }
            else if (match.Groups["bare"].Success)
            {
                value = match.Groups["bare"].Value;
            }

            result.TryAdd(normalizedName, value);
        }

        return result;
    }

    private static string NormalizeBoundAttributeName(string name)
    {
        var candidate = name.Trim();
        if (candidate.StartsWith("[(", StringComparison.Ordinal) &&
            candidate.EndsWith(")]", StringComparison.Ordinal) &&
            candidate.Length > 4)
        {
            return candidate[2..^2];
        }

        if (candidate.StartsWith("[", StringComparison.Ordinal) &&
            candidate.EndsWith("]", StringComparison.Ordinal) &&
            candidate.Length > 2)
        {
            candidate = candidate[1..^1];
        }

        if (candidate.StartsWith("bind-", StringComparison.OrdinalIgnoreCase))
        {
            candidate = candidate[5..];
        }

        return candidate;
    }

    private static bool IsInsideHtmlComment(string text, int position)
    {
        var open = text.LastIndexOf("<!--", position, StringComparison.Ordinal);
        if (open < 0)
        {
            return false;
        }

        var close = text.LastIndexOf("-->", position, StringComparison.Ordinal);
        return close < open;
    }

    private sealed record ElementFrame(string Name, string Attributes);
}
