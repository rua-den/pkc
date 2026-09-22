using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Frontend;

internal sealed class AngularRenderedMemberVisibilityEnricher
{
    private static readonly Regex ComponentTemplateRegex = new(
        @"@Component\s*\(\s*\{(?<before>[\s\S]*?)\btemplate\s*:\s*`(?<body>[\s\S]*?)`(?<after>[\s\S]*?)\}\s*\)\s*(?:export\s+)?class\s+(?<component>[A-Z][A-Za-z0-9_$]*Component)\b",
        RegexOptions.Compiled);

    private static readonly Regex AtIfRegex = new(
        @"@if\s*\((?<condition>[^\r\n{}]+)\)\s*\{",
        RegexOptions.Compiled);

    private static readonly Regex AngularControlBlockRegex = new(
        @"@(?:(?<name>if|for|switch|case)\s*\([^{}\r\n]*\)|(?<name>defer|placeholder|loading|error)\b(?:\s*\([^{}\r\n]*\))?|(?<name>else)\b[^{}\r\n]*|(?<name>empty|default)\b)\s*\{",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex HtmlElementTagRegex = new(
        @"<\s*(?<closing>/)?\s*(?<name>[A-Za-z][A-Za-z0-9:-]*)\b(?<attrs>(?:[^""'<>]|""[^""]*""|'[^']*')*)>",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly HashSet<string> VoidHtmlElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr"
    };

    public async Task<FactDocument> EnrichAsync(
        string repositoryPath,
        FactDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        ArgumentNullException.ThrowIfNull(document);

        var renders = document.Facts
            .Where(fact => fact.Kind == "ui-member-render")
            .Where(fact => fact.Metadata.ContainsKey("renderAuthority"))
            .ToArray();
        if (renders.Length == 0)
        {
            return document;
        }

        var root = Path.GetFullPath(repositoryPath);
        var cache = new Dictionary<string, string>(StringComparer.Ordinal);
        var facts = document.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        var relations = document.Relations.ToList();

        foreach (var render in renders)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!render.Metadata.TryGetValue("component", out var component) ||
                string.IsNullOrWhiteSpace(component) ||
                !render.Metadata.TryGetValue("componentIdentity", out var componentIdentity) ||
                string.IsNullOrWhiteSpace(componentIdentity) ||
                !render.Metadata.TryGetValue("member", out var member) ||
                string.IsNullOrWhiteSpace(member))
            {
                continue;
            }

            if (!cache.TryGetValue(render.Source.Path, out var text))
            {
                var fullPath = Path.Combine(
                    root,
                    render.Source.Path.Replace('/', Path.DirectorySeparatorChar));
                text = File.Exists(fullPath)
                    ? await File.ReadAllTextAsync(fullPath, cancellationToken)
                    : string.Empty;
                cache[render.Source.Path] = text;
            }

            if (string.IsNullOrEmpty(text) ||
                !TryResolveVisibility(
                    text,
                    component,
                    member,
                    render.Source.StartLine,
                    out var condition,
                    out var conditionIndex,
                    out var conditionLength))
            {
                continue;
            }

            var source = GetLocation(
                render.Source.Path,
                text,
                conditionIndex,
                conditionLength);
            var visibility = new EvidenceFact(
                $"ui-wire:{render.Source.Path}:{source.StartLine}:member-visibility:{component}:{member}:{render.Source.StartLine}",
                "ui-member-visibility",
                $"{component}.{member} visible when {condition}",
                component,
                source,
                [],
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["component"] = component,
                    ["componentIdentity"] = componentIdentity,
                    ["member"] = member,
                    ["renderFactId"] = render.Id,
                    ["condition"] = condition,
                    ["analysisMode"] = "angular-bounded-render-visibility",
                    ["analysisConfidence"] = "high",
                    ["proof"] = "authoritative-simple-interpolation+single-enclosing-at-if"
                });

            facts[visibility.Id] = visibility;
            relations.Add(new EvidenceRelation(
                render.Id,
                "controlled-by-visibility",
                visibility.Id,
                source));
        }

        return document with
        {
            Facts = facts.Values.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            Relations = relations
                .GroupBy(RelationKey, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static bool TryResolveVisibility(
        string text,
        string component,
        string member,
        int sourceLine,
        out string condition,
        out int conditionIndex,
        out int conditionLength)
    {
        condition = string.Empty;
        conditionIndex = -1;
        conditionLength = 0;

        var matches = new List<(string Condition, int Index, int Length)>();
        foreach (Match template in ComponentTemplateRegex.Matches(text))
        {
            if (!IsActiveCodePosition(text, template.Index) ||
                !string.Equals(template.Groups["component"].Value, component, StringComparison.Ordinal))
            {
                continue;
            }

            var body = template.Groups["body"];
            var interpolationRegex = new Regex(
                @"\{\{\s*" + Regex.Escape(member) + @"\s*\}\}",
                RegexOptions.CultureInvariant);
            foreach (Match interpolation in interpolationRegex.Matches(body.Value))
            {
                var absoluteInterpolationIndex = body.Index + interpolation.Index;
                var line = 1 + text.AsSpan(0, absoluteInterpolationIndex).Count('\n');
                if (line != sourceLine)
                {
                    continue;
                }

                var containing = AtIfRegex.Matches(body.Value)
                    .Cast<Match>()
                    .Where(atIf =>
                        atIf.Index < interpolation.Index &&
                        !IsInsideHtmlComment(body.Value, atIf.Index) &&
                        !IsInsideHtmlTag(body.Value, atIf.Index))
                    .Select(atIf => new
                    {
                        Match = atIf,
                        OpenBrace = atIf.Index + atIf.Length - 1,
                        CloseBrace = FindMatchingBrace(body.Value, atIf.Index + atIf.Length - 1)
                    })
                    .Where(candidate =>
                        candidate.CloseBrace >= 0 &&
                        interpolation.Index > candidate.OpenBrace &&
                        interpolation.Index < candidate.CloseBrace)
                    .ToArray();

                if (containing.Length != 1 ||
                    HasUnsupportedStructuralDirectiveAncestor(body.Value, interpolation.Index))
                {
                    continue;
                }

                var enclosingControlBlocks = AngularControlBlockRegex.Matches(body.Value)
                    .Cast<Match>()
                    .Where(block =>
                        block.Index < interpolation.Index &&
                        !IsInsideHtmlComment(body.Value, block.Index) &&
                        !IsInsideHtmlTag(body.Value, block.Index))
                    .Select(block => new
                    {
                        Match = block,
                        OpenBrace = block.Index + block.Value.LastIndexOf('{'),
                        CloseBrace = FindMatchingBrace(
                            body.Value,
                            block.Index + block.Value.LastIndexOf('{'))
                    })
                    .Where(candidate =>
                        candidate.CloseBrace >= 0 &&
                        interpolation.Index > candidate.OpenBrace &&
                        interpolation.Index < candidate.CloseBrace)
                    .ToArray();

                if (enclosingControlBlocks.Length != 1 ||
                    enclosingControlBlocks[0].Match.Index != containing[0].Match.Index ||
                    !string.Equals(
                        enclosingControlBlocks[0].Match.Groups["name"].Value,
                        "if",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                var atIf = containing[0].Match;
                var normalizedCondition = Regex.Replace(
                    atIf.Groups["condition"].Value,
                    @"\s+",
                    " ").Trim();
                if (string.IsNullOrWhiteSpace(normalizedCondition))
                {
                    continue;
                }

                matches.Add((
                    normalizedCondition,
                    body.Index + atIf.Index,
                    atIf.Length));
            }
        }

        if (matches.Count != 1)
        {
            return false;
        }

        condition = matches[0].Condition;
        conditionIndex = matches[0].Index;
        conditionLength = matches[0].Length;
        return true;
    }

    private static bool HasUnsupportedStructuralDirectiveAncestor(string templateBody, int position)
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

            ancestors.Push(new HtmlElementFrame(
                name,
                HasUnsupportedStructuralDirectiveAttribute(attrs)));
        }

        return ancestors.Any(ancestor => ancestor.HasUnsupportedStructuralDirective);
    }

    private static bool HasUnsupportedStructuralDirectiveAttribute(string attributes)
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
            if (name.StartsWith("*", StringComparison.Ordinal))
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

    private static int FindMatchingBrace(string text, int openBrace)
    {
        var depth = 0;

        for (var index = openBrace; index < text.Length; index++)
        {
            if (index + 3 < text.Length &&
                text[index] == '<' &&
                text[index + 1] == '!' &&
                text[index + 2] == '-' &&
                text[index + 3] == '-')
            {
                var commentEnd = text.IndexOf("-->", index + 4, StringComparison.Ordinal);
                if (commentEnd < 0)
                {
                    return -1;
                }

                index = commentEnd + 2;
                continue;
            }

            if (text[index] == '<' && TryGetHtmlTagEnd(text, index, out var tagEnd))
            {
                index = tagEnd;
                continue;
            }

            if (index + 1 < text.Length && text[index] == '{' && text[index + 1] == '{')
            {
                if (!TryGetInterpolationEnd(text, index, out var interpolationEnd))
                {
                    return -1;
                }

                index = interpolationEnd;
                continue;
            }

            var current = text[index];
            if (current == '{')
            {
                depth++;
                continue;
            }

            if (current != '}')
            {
                continue;
            }

            depth--;
            if (depth == 0)
            {
                return index;
            }

            if (depth < 0)
            {
                return -1;
            }
        }

        return -1;
    }

    private static bool TryGetHtmlTagEnd(string text, int start, out int end)
    {
        end = -1;
        var match = HtmlElementTagRegex.Match(text, start);
        if (!match.Success || match.Index != start)
        {
            return false;
        }

        end = match.Index + match.Length - 1;
        return true;
    }

    private static bool TryGetInterpolationEnd(string text, int start, out int end)
    {
        end = -1;
        if (start + 1 >= text.Length || text[start] != '{' || text[start + 1] != '{')
        {
            return false;
        }

        var nestedBraceDepth = 0;
        var quote = '\0';
        for (var index = start + 2; index < text.Length; index++)
        {
            var current = text[index];
            if (quote != '\0')
            {
                if (current == '\\')
                {
                    index++;
                    continue;
                }

                if (current == quote)
                {
                    quote = '\0';
                }

                continue;
            }

            if (current is '\'' or '"' or '`')
            {
                quote = current;
                continue;
            }

            if (current == '{')
            {
                nestedBraceDepth++;
                continue;
            }

            if (current != '}')
            {
                continue;
            }

            if (nestedBraceDepth > 0)
            {
                nestedBraceDepth--;
                continue;
            }

            if (index + 1 < text.Length && text[index + 1] == '}')
            {
                end = index + 1;
                return true;
            }
        }

        return false;
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
        foreach (Match tag in HtmlElementTagRegex.Matches(text))
        {
            if (tag.Index > position)
            {
                break;
            }

            if (position >= tag.Index && position < tag.Index + tag.Length)
            {
                return true;
            }
        }

        return false;
    }

    private static SourceLocation GetLocation(
        string path,
        string text,
        int index,
        int length)
    {
        var startLine = 1 + text.AsSpan(0, index).Count('\n');
        var endIndex = Math.Min(text.Length, index + Math.Max(length, 1));
        var endLine = 1 + text.AsSpan(0, endIndex).Count('\n');
        return new SourceLocation(path, startLine, endLine);
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

    private static string RelationKey(EvidenceRelation relation) =>
        $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}";

    private sealed record HtmlElementFrame(string Name, bool HasUnsupportedStructuralDirective);

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
