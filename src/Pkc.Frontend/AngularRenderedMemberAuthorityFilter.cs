using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Frontend;

internal sealed class AngularRenderedMemberAuthorityFilter
{
    private static readonly Regex ComponentTemplateRegex = new(
        @"@Component\s*\(\s*\{(?<before>[\s\S]*?)\btemplate\s*:\s*`(?<body>[\s\S]*?)`(?<after>[\s\S]*?)\}\s*\)\s*(?:export\s+)?class\s+(?<component>[A-Z][A-Za-z0-9_$]*Component)\b",
        RegexOptions.Compiled);

    private static readonly Regex NgTemplateTagRegex = new(
        @"<\s*(?<closing>/)?\s*ng-template\b[^>]*>",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex HtmlElementTagRegex = new(
        @"<\s*(?<closing>/)?\s*(?<name>[A-Za-z][A-Za-z0-9:-]*)\b(?<attrs>(?:[^""'<>]|""[^""]*""|'[^']*')*)>",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex StaticDisplayNoneRegex = new(
        @"^none\s*(?:!important\s*)?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex StaticVisibilityHiddenRegex = new(
        @"^hidden\s*(?:!important\s*)?$",
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
        var kept = new List<EvidenceFact>(document.Facts.Count);
        foreach (var fact in document.Facts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (fact.Kind != "ui-member-render")
            {
                kept.Add(fact);
                continue;
            }

            if (!fact.Metadata.TryGetValue("component", out var component) ||
                string.IsNullOrWhiteSpace(component) ||
                !fact.Metadata.TryGetValue("member", out var member) ||
                string.IsNullOrWhiteSpace(member))
            {
                continue;
            }

            if (!cache.TryGetValue(fact.Source.Path, out var text))
            {
                var fullPath = Path.Combine(
                    root,
                    fact.Source.Path.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(fullPath))
                {
                    cache[fact.Source.Path] = string.Empty;
                    continue;
                }

                text = await File.ReadAllTextAsync(fullPath, cancellationToken);
                cache[fact.Source.Path] = text;
            }

            if (string.IsNullOrEmpty(text) ||
                CountAuthoritativeMatches(text, component, member, fact.Source.StartLine) != 1)
            {
                continue;
            }

            var metadata = new Dictionary<string, string>(fact.Metadata, StringComparer.Ordinal)
            {
                ["renderAuthority"] = "active-component-inline-template+exact-simple-interpolation"
            };
            kept.Add(fact with { Metadata = metadata });
        }

        var keptIds = kept.Select(fact => fact.Id).ToHashSet(StringComparer.Ordinal);
        return document with
        {
            Facts = kept.ToArray(),
            Relations = document.Relations
                .Where(relation => keptIds.Contains(relation.FromFactId))
                .ToArray()
        };
    }

    private static int CountAuthoritativeMatches(
        string text,
        string component,
        string member,
        int sourceLine)
    {
        var count = 0;
        foreach (Match match in ComponentTemplateRegex.Matches(text))
        {
            if (!IsActiveCodePosition(text, match.Index) ||
                !string.Equals(match.Groups["component"].Value, component, StringComparison.Ordinal))
            {
                continue;
            }

            var body = match.Groups["body"];
            var interpolation = new Regex(
                @"\{\{\s*" + Regex.Escape(member) + @"\s*\}\}",
                RegexOptions.CultureInvariant);
            foreach (Match memberMatch in interpolation.Matches(body.Value))
            {
                var absoluteIndex = body.Index + memberMatch.Index;
                var line = 1 + text.AsSpan(0, absoluteIndex).Count('\n');
                if (line == sourceLine &&
                    !IsInsideHtmlComment(body.Value, memberMatch.Index) &&
                    !IsInsideHtmlTag(body.Value, memberMatch.Index) &&
                    !IsInsideInertNgTemplate(body.Value, memberMatch.Index) &&
                    !IsInsideStaticallyNonRenderedHtmlAncestor(body.Value, memberMatch.Index))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static bool IsInsideStaticallyNonRenderedHtmlAncestor(string templateBody, int position)
    {
        var ancestors = new Stack<HtmlElementFrame>();
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
                if (VoidHtmlElements.Contains(name) ||
                    ancestors.Count == 0 ||
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

            ancestors.Push(new HtmlElementFrame(name, HasStaticPresentationSuppression(attrs)));
        }

        return ancestors.Any(ancestor => ancestor.IsStaticallyNonRendered);
    }

    private static bool HasStaticPresentationSuppression(string attributes)
    {
        var index = 0;
        while (index < attributes.Length)
        {
            while (index < attributes.Length && char.IsWhiteSpace(attributes[index]))
            {
                index++;
            }

            if (index >= attributes.Length || attributes[index] == '/')
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
            while (index < attributes.Length && char.IsWhiteSpace(attributes[index]))
            {
                index++;
            }

            var hasValue = index < attributes.Length && attributes[index] == '=';
            string? value = null;
            if (hasValue)
            {
                index++;
                while (index < attributes.Length && char.IsWhiteSpace(attributes[index]))
                {
                    index++;
                }

                if (index < attributes.Length && attributes[index] is '\'' or '"')
                {
                    var quote = attributes[index++];
                    var valueStart = index;
                    while (index < attributes.Length && attributes[index] != quote)
                    {
                        index++;
                    }

                    value = attributes[valueStart..Math.Min(index, attributes.Length)];
                    if (index < attributes.Length)
                    {
                        index++;
                    }
                }
                else
                {
                    var valueStart = index;
                    while (index < attributes.Length &&
                           !char.IsWhiteSpace(attributes[index]) &&
                           attributes[index] != '/')
                    {
                        index++;
                    }

                    value = attributes[valueStart..index];
                }
            }

            var isStaticValue = value is not null &&
                                !value.Contains("{{", StringComparison.Ordinal) &&
                                !value.Contains("}}", StringComparison.Ordinal);

            if (string.Equals(name, "hidden", StringComparison.OrdinalIgnoreCase))
            {
                if (!hasValue || isStaticValue)
                {
                    return true;
                }

                continue;
            }

            if (string.Equals(name, "style", StringComparison.OrdinalIgnoreCase) &&
                hasValue &&
                isStaticValue &&
                HasStaticInlinePresentationSuppression(value!))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasStaticInlinePresentationSuppression(string style)
    {
        foreach (var declaration in style.Split(';'))
        {
            var separator = declaration.IndexOf(':');
            if (separator <= 0)
            {
                continue;
            }

            var property = declaration[..separator].Trim();
            var value = declaration[(separator + 1)..].Trim();
            if (string.Equals(property, "display", StringComparison.OrdinalIgnoreCase) &&
                StaticDisplayNoneRegex.IsMatch(value))
            {
                return true;
            }

            if (string.Equals(property, "visibility", StringComparison.OrdinalIgnoreCase) &&
                StaticVisibilityHiddenRegex.IsMatch(value))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsInsideInertNgTemplate(string templateBody, int position)
    {
        var depth = 0;
        foreach (Match tag in NgTemplateTagRegex.Matches(templateBody))
        {
            if (tag.Index >= position)
            {
                break;
            }

            if (IsInsideHtmlComment(templateBody, tag.Index))
            {
                continue;
            }

            if (tag.Groups["closing"].Success)
            {
                if (depth > 0)
                {
                    depth--;
                }

                continue;
            }

            if (!tag.Value.TrimEnd().EndsWith("/>", StringComparison.Ordinal))
            {
                depth++;
            }
        }

        return depth > 0;
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

    private static bool IsInsideHtmlTag(string text, int position)
    {
        var open = text.LastIndexOf('<', position);
        if (open < 0)
        {
            return false;
        }

        var close = text.LastIndexOf('>', position);
        return close < open;
    }

    private static bool IsActiveCodePosition(string text, int position)
    {
        var state = LexicalState.Code;
        for (var index = 0; index < position; index++)
        {
            var current = text[index];
            var next = index + 1 < text.Length ? text[index + 1] : '\0';
            switch (state)
            {
                case LexicalState.Code:
                    if (current == '/' && next == '/')
                    {
                        state = LexicalState.LineComment;
                        index++;
                    }
                    else if (current == '/' && next == '*')
                    {
                        state = LexicalState.BlockComment;
                        index++;
                    }
                    else if (current == '\'') state = LexicalState.SingleQuotedString;
                    else if (current == '"') state = LexicalState.DoubleQuotedString;
                    else if (current == '`') state = LexicalState.TemplateLiteral;
                    break;
                case LexicalState.LineComment:
                    if (current is '\r' or '\n') state = LexicalState.Code;
                    break;
                case LexicalState.BlockComment:
                    if (current == '*' && next == '/')
                    {
                        state = LexicalState.Code;
                        index++;
                    }
                    break;
                case LexicalState.SingleQuotedString:
                    if (current == '\\') index++;
                    else if (current == '\'') state = LexicalState.Code;
                    break;
                case LexicalState.DoubleQuotedString:
                    if (current == '\\') index++;
                    else if (current == '"') state = LexicalState.Code;
                    break;
                case LexicalState.TemplateLiteral:
                    if (current == '\\') index++;
                    else if (current == '`') state = LexicalState.Code;
                    break;
            }
        }

        return state == LexicalState.Code;
    }

    private sealed record HtmlElementFrame(string Name, bool IsStaticallyNonRendered);

    private enum LexicalState
    {
        Code,
        LineComment,
        BlockComment,
        SingleQuotedString,
        DoubleQuotedString,
        TemplateLiteral
    }
}
