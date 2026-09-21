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
                    .Where(atIf => atIf.Index < interpolation.Index)
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

                if (containing.Length != 1)
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

    private static int FindMatchingBrace(string text, int openBrace)
    {
        var depth = 0;
        var quote = '\0';

        for (var index = openBrace; index < text.Length; index++)
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

            if (current is '\'' or '"')
            {
                quote = current;
                continue;
            }

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
        }

        return -1;
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
