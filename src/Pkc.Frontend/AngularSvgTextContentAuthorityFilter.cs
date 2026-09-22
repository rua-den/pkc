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

    private static readonly HashSet<string> SupportedInlineTextElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "tspan", "a"
    };

    private static readonly HashSet<string> SupportedTextParents = new(StringComparer.OrdinalIgnoreCase)
    {
        "svg", "g", "a"
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

            ancestors.Push(new ElementFrame(name, attributes, elementNamespace, childNamespace));
        }

        var frames = ancestors.ToArray();
        var svgFrames = frames
            .Where(frame => frame.ElementNamespace == MarkupNamespace.Svg)
            .ToArray();
        if (svgFrames.Length == 0)
        {
            return true;
        }

        if (svgFrames.Any(frame =>
                string.Equals(frame.Name, "switch", StringComparison.OrdinalIgnoreCase) ||
                HasUnprovenConditionalProcessing(frame.Attributes)))
        {
            return false;
        }

        var currentNamespace = ancestors.Count == 0
            ? MarkupNamespace.Html
            : ancestors.Peek().ChildNamespace;
        if (currentNamespace != MarkupNamespace.Svg)
        {
            return true;
        }

        var nearestSvgIndex = Array.FindIndex(
            frames,
            frame =>
                frame.ElementNamespace == MarkupNamespace.Svg &&
                string.Equals(frame.Name, "svg", StringComparison.OrdinalIgnoreCase));
        if (nearestSvgIndex < 0)
        {
            return false;
        }

        var textIndex = -1;
        for (var index = 0; index < nearestSvgIndex; index++)
        {
            var frame = frames[index];
            if (frame.ElementNamespace != MarkupNamespace.Svg)
            {
                return false;
            }

            if (string.Equals(frame.Name, "text", StringComparison.OrdinalIgnoreCase))
            {
                textIndex = index;
                break;
            }

            if (!SupportedInlineTextElements.Contains(frame.Name))
            {
                return false;
            }
        }

        if (textIndex < 0 || textIndex + 1 >= frames.Length)
        {
            return false;
        }

        var textParent = frames[textIndex + 1];
        return textParent.ElementNamespace == MarkupNamespace.Svg &&
               SupportedTextParents.Contains(textParent.Name);
    }

    private static bool HasUnprovenConditionalProcessing(string attributes)
    {
        var index = 0;
        while (index < attributes.Length)
        {
            while (index < attributes.Length && char.IsWhiteSpace(attributes[index]))
            {
                index++;
            }

            if (index >= attributes.Length)
            {
                break;
            }

            if (attributes[index] == '/')
            {
                index++;
                continue;
            }

            var nameStart = index;
            while (index < attributes.Length &&
                   !char.IsWhiteSpace(attributes[index]) &&
                   attributes[index] is not '=' and not '/')
            {
                index++;
            }

            if (index == nameStart)
            {
                index++;
                continue;
            }

            var name = attributes[nameStart..index];
            if (IsConditionalProcessingAttribute(name))
            {
                return true;
            }

            while (index < attributes.Length && char.IsWhiteSpace(attributes[index]))
            {
                index++;
            }

            if (index >= attributes.Length || attributes[index] != '=')
            {
                continue;
            }

            index++;
            while (index < attributes.Length && char.IsWhiteSpace(attributes[index]))
            {
                index++;
            }

            if (index < attributes.Length && attributes[index] is '\'' or '"')
            {
                var quote = attributes[index++];
                while (index < attributes.Length && attributes[index] != quote)
                {
                    index++;
                }

                if (index < attributes.Length)
                {
                    index++;
                }
            }
            else
            {
                while (index < attributes.Length &&
                       !char.IsWhiteSpace(attributes[index]) &&
                       attributes[index] != '/')
                {
                    index++;
                }
            }
        }

        return false;
    }

    private static bool IsConditionalProcessingAttribute(string name) =>
        string.Equals(name, "systemLanguage", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(name, "requiredExtensions", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(name, "requiredFeatures", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(name, "[systemLanguage]", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(name, "[requiredExtensions]", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(name, "[requiredFeatures]", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(name, "[attr.systemLanguage]", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(name, "[attr.requiredExtensions]", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(name, "[attr.requiredFeatures]", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(name, "bind-systemLanguage", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(name, "bind-requiredExtensions", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(name, "bind-requiredFeatures", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(name, "bind-attr.systemLanguage", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(name, "bind-attr.requiredExtensions", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(name, "bind-attr.requiredFeatures", StringComparison.OrdinalIgnoreCase);

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
        string Attributes,
        MarkupNamespace ElementNamespace,
        MarkupNamespace ChildNamespace);

    private enum MarkupNamespace
    {
        Html,
        Svg
    }
}
