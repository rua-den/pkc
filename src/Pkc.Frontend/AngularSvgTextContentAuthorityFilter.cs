using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Frontend;

internal sealed class AngularSvgTextContentAuthorityFilter
{
    private static readonly Regex ComponentTemplateRegex = new(
        @"@Component\s*\(\s*\{(?<before>[\s\S]*?)\btemplate\s*:\s*`(?<body>[\s\S]*?)`(?<after>[\s\S]*?)\}\s*\)\s*(?:export\s+)?class\s+(?<component>[A-Z][A-Za-z0-9_$]*Component)\b",
        RegexOptions.Compiled);

    private static readonly Regex HtmlElementTagRegex = new(
        @"<\s*(?<closing>/)?\s*(?<name>[A-Za-z][A-Za-z0-9:-]*)\b(?<attrs>(?:[^""'<>]|""[^""]*""|'[^']*')*)>",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

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
                !HasSupportedRenderContext(text, component, member, fact.Source.StartLine))
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

    private static bool HasSupportedRenderContext(
        string text,
        string component,
        string member,
        int sourceLine)
    {
        var matches = 0;
        var supported = 0;

        foreach (Match template in ComponentTemplateRegex.Matches(text))
        {
            if (!string.Equals(template.Groups["component"].Value, component, StringComparison.Ordinal))
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
                if (IsSupportedRenderPosition(body.Value, interpolation.Index))
                {
                    supported++;
                }
            }
        }

        return matches == 1 && supported == 1;
    }

    private static bool IsSupportedRenderPosition(string templateBody, int position)
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
                    return false;
                }

                ancestors.Pop();
                continue;
            }

            var parentNamespace = ancestors.Count == 0
                ? MarkupNamespace.Html
                : ancestors.Peek().ChildNamespace;
            var elementNamespace = parentNamespace;
            var childNamespace = parentNamespace;

            if (parentNamespace == MarkupNamespace.Html &&
                string.Equals(name, "svg", StringComparison.OrdinalIgnoreCase))
            {
                elementNamespace = MarkupNamespace.Svg;
                childNamespace = MarkupNamespace.Svg;
            }
            else if (parentNamespace == MarkupNamespace.Svg &&
                     string.Equals(name, "foreignObject", StringComparison.OrdinalIgnoreCase))
            {
                elementNamespace = MarkupNamespace.Svg;
                childNamespace = MarkupNamespace.Html;
            }

            var attributes = tag.Groups["attrs"].Value;
            var selfClosing = attributes.TrimEnd().EndsWith("/", StringComparison.Ordinal);
            if (selfClosing ||
                (elementNamespace == MarkupNamespace.Html && VoidHtmlElements.Contains(name)))
            {
                continue;
            }

            ancestors.Push(new ElementFrame(name, elementNamespace, childNamespace));
        }

        if (!ancestors.Any(frame => frame.ElementNamespace == MarkupNamespace.Svg))
        {
            return true;
        }

        var currentNamespace = ancestors.Count == 0
            ? MarkupNamespace.Html
            : ancestors.Peek().ChildNamespace;

        // V0.4.7 does not claim native SVG paint/layout authority. Native SVG
        // text can be structurally present while producing no visible glyphs
        // because of paint, masking, clipping, filters, conditional processing,
        // inherited presentation state, or other SVG rendering semantics. Keep
        // that surface fail-closed until a later checkpoint proves it.
        // HTML inside foreignObject is still handled by the normal HTML path.
        return currentNamespace != MarkupNamespace.Svg;
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

    private sealed record ElementFrame(
        string Name,
        MarkupNamespace ElementNamespace,
        MarkupNamespace ChildNamespace);

    private enum MarkupNamespace
    {
        Html,
        Svg
    }
}
