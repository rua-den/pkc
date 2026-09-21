using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Frontend;

internal sealed class AngularRenderedMemberVisibilityAuthorityFilter
{
    private static readonly Regex AtIfStatementRegex = new(
        @"^@if\s*\(",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

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
        var linesByPath = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var rejected = new HashSet<string>(StringComparer.Ordinal);

        foreach (var visibility in visibilities)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!linesByPath.TryGetValue(visibility.Source.Path, out var lines))
            {
                var fullPath = Path.Combine(
                    root,
                    visibility.Source.Path.Replace('/', Path.DirectorySeparatorChar));
                lines = File.Exists(fullPath)
                    ? await File.ReadAllLinesAsync(fullPath, cancellationToken)
                    : [];
                linesByPath[visibility.Source.Path] = lines;
            }

            var lineIndex = visibility.Source.StartLine - 1;
            if (lineIndex < 0 ||
                lineIndex >= lines.Length ||
                !AtIfStatementRegex.IsMatch(lines[lineIndex].TrimStart()))
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
}
