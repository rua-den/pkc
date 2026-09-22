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
        foreach (var file in Directory.EnumerateFiles(root, "*.ts", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relativePath = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (!FrontendSourceScope.IsProductSource(relativePath))
            {
                continue;
            }

            var text = await File.ReadAllTextAsync(file, cancellationToken);
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

        return selectors.OrderBy(value => value, StringComparer.Ordinal).ToArray();
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
