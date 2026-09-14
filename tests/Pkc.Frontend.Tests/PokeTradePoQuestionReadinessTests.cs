using Pkc.Core;
using Pkc.CSharp;
using Pkc.Frontend;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.Frontend.Tests;

public sealed class PokeTradePoQuestionReadinessTests
{
    [Fact]
    public async Task Mewtwo_catalog_visibility_is_answerable_from_compiled_knowledge()
    {
        var repositoryRoot = FindRepositoryRoot();
        var sample = Path.Combine(repositoryRoot, "samples", "PokeTradeSystem");

        var csharp = await new CSharpEvidenceScanner().ScanAsync(sample);
        var frontend = await new AngularFrontendAdapter().ScanAsync(sample);
        var merged = Merge(csharp, frontend);

        var configured = Assert.Single(merged.Facts, fact =>
            fact.Kind == "configured-object" &&
            fact.Metadata.TryGetValue("assignments", out var assignments) &&
            assignments.Contains("Name = \"Mewtwo VSTAR\"", StringComparison.Ordinal));

        Assert.Equal("_cards", configured.Metadata["source"]);
        Assert.Contains("IsPublished = true", configured.Metadata["assignments"], StringComparison.Ordinal);
        Assert.Contains("WebEnabled = true", configured.Metadata["assignments"], StringComparison.Ordinal);
        Assert.Contains("SaleStartsAt = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero)", configured.Metadata["assignments"], StringComparison.Ordinal);
        Assert.Contains("SaleEndsAt = null", configured.Metadata["assignments"], StringComparison.Ordinal);
        Assert.Contains("Stock = 5", configured.Metadata["assignments"], StringComparison.Ordinal);

        var candidate = Assert.Single(
            new CrossStackFeatureCandidateBuilder().Build(merged).Candidates,
            candidate => candidate.Name.Contains("Cards Get Cards", StringComparison.Ordinal));
        var knowledge = await new GroundedKnowledgeSynthesizer().SynthesizeAsync(candidate);

        Assert.Contains(knowledge.Rules, rule =>
            rule.Contains("Includes items from `_cards` only when", StringComparison.Ordinal) &&
            rule.Contains("card.IsPublished", StringComparison.Ordinal) &&
            rule.Contains("card.WebEnabled", StringComparison.Ordinal) &&
            rule.Contains("card.SaleStartsAt <= now", StringComparison.Ordinal) &&
            rule.Contains("card.SaleEndsAt == null || now < card.SaleEndsAt", StringComparison.Ordinal) &&
            rule.Contains("card.Stock > 0", StringComparison.Ordinal));

        Assert.Contains(knowledge.Rules, rule =>
            rule.Contains("Configured item in `_cards`", StringComparison.Ordinal) &&
            rule.Contains("Mewtwo VSTAR", StringComparison.Ordinal) &&
            rule.Contains("2026, 9, 1", StringComparison.Ordinal));

        Assert.Contains(knowledge.UiToBackend, step =>
            step.Contains("result of API method `getCards`", StringComparison.Ordinal) &&
            step.Contains("`cards`", StringComparison.Ordinal));
        Assert.Contains(knowledge.UiToBackend, step =>
            step.Contains("renders each `card` from `cards`", StringComparison.Ordinal));
    }

    private static FactDocument Merge(params FactDocument[] documents)
    {
        var facts = documents
            .SelectMany(document => document.Facts)
            .GroupBy(fact => fact.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(fact => fact.Id, StringComparer.Ordinal)
            .ToArray();
        var factIds = facts.Select(fact => fact.Id).ToHashSet(StringComparer.Ordinal);

        var relations = documents
            .SelectMany(document => document.Relations)
            .Where(relation => factIds.Contains(relation.FromFactId))
            .GroupBy(relation => $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}", StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
            .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
            .ThenBy(relation => relation.Target, StringComparer.Ordinal)
            .ToArray();

        return new FactDocument("po-readiness-test", facts, relations);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "PKC.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate PKC.sln from the test output directory.");
    }
}
