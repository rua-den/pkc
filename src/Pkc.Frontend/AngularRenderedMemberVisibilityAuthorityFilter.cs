using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Frontend;

internal sealed class AngularRenderedMemberVisibilityAuthorityFilter
{
    private static readonly Regex AtIfStatementRegex = new(
        @"^@if\s*\(",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex HtmlElementTagRegex = new(
        @"<\s*(?<closing>/)?\s*(?<name>[A-Za-z][A-Za-z0-9:-]*)\b(?<attrs>(?:[^""'<>]|""[^""]*""|'[^']*')*)>",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public async Task<FactDocument> FilterAsync(
        string repositoryPath,
        FactDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        ArgumentNullException.ThrowIfNull(document);

        var visibilities = document.Facts
            .Where(fact => fact.Kind == "ui-member-visibility")
            .ToArray();
        if (visibilities.Length == 0)
        {
            return document;
        }

        var root = Path.GetFullPath(repositoryPath);
        var textByPath = new Dictionary<string, string>(StringComparer.Ordinal);
        var rejected = new HashSet<string>(StringComparer.Ordinal);

        foreach (var visibility in visibilities)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!textByPath.TryGetValue(visibility.Source.Path, out var text))
            {
                var fullPath = Path.Combine(
                    root,
                    visibility.Source.Path.Replace('/', Path.DirectorySeparatorChar));
                text = File.Exists(fullPath)
                    ? await File.ReadAllTextAsync(fullPath, cancellationToken)
                    : string.Empty;
                textByPath[visibility.Source.Path] = text;
            }

            if (!TryGetLineControlPosition(
                    text,
                    visibility.Source.StartLine,
                    out var controlPosition) ||
                IsInsideHtmlComment(text, controlPosition) ||
                IsInsideHtmlTag(text, controlPosition))
            {
                rejected.Add(visibility.Id);
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

    private static bool TryGetLineControlPosition(
        string text,
        int sourceLine,
        out int controlPosition)
    {
        controlPosition = -1;
        if (sourceLine <= 0 || string.IsNullOrEmpty(text))
        {
            return false;
        }

        var lineStart = 0;
        for (var line = 1; line < sourceLine; line++)
        {
            var newline = text.IndexOf('\n', lineStart);
            if (newline < 0)
            {
                return false;
            }

            lineStart = newline + 1;
        }

        var lineEnd = text.IndexOf('\n', lineStart);
        if (lineEnd < 0)
        {
            lineEnd = text.Length;
        }

        var lineText = text[lineStart..lineEnd];
        var leadingWhitespace = lineText.Length - lineText.TrimStart().Length;
        var trimmed = lineText[leadingWhitespace..];
        if (!AtIfStatementRegex.IsMatch(trimmed))
        {
            return false;
        }

        controlPosition = lineStart + leadingWhitespace;
        return true;
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
}
