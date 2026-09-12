using Pkc.Core;

namespace Pkc.Knowledge;

public sealed class EvidenceAwareKnowledgeSynthesizer : IKnowledgeSynthesizer
{
    private const string AspNetSignIn =
        "Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignInAsync";

    private const string AspNetSignOut =
        "Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignOutAsync";

    private readonly GroundedKnowledgeSynthesizer _inner = new();

    public async ValueTask<FeatureKnowledge> SynthesizeAsync(
        FeatureCandidate candidate,
        CancellationToken cancellationToken = default)
    {
        var knowledge = await _inner.SynthesizeAsync(candidate, cancellationToken);
        var responseFacts = candidate.Facts
            .Where(fact => fact.Kind == "endpoint-response")
            .ToArray();

        var rules = knowledge.Rules
            .Concat(responseFacts.Select(DescribeResponseRule))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var sideEffects = knowledge.SideEffects
            .Concat(BuildSemanticSideEffects(candidate))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var evidence = knowledge.Evidence
            .Concat(responseFacts.Select(fact => new KnowledgeEvidence(
                fact.Id,
                fact.Kind,
                $"Endpoint response: {DescribeResponseRule(fact)}",
                fact.Source)))
            .GroupBy(item => item.FactId, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => item.Source.Path, StringComparer.Ordinal)
            .ThenBy(item => item.Source.StartLine)
            .ToArray();

        return knowledge with
        {
            Rules = rules,
            SideEffects = sideEffects,
            Evidence = evidence
        };
    }

    private static string DescribeResponseRule(EvidenceFact fact)
    {
        if (fact.Metadata.TryGetValue("conditionExpression", out var condition) &&
            fact.Metadata.TryGetValue("whenTrue", out var whenTrue) &&
            fact.Metadata.TryGetValue("whenFalse", out var whenFalse))
        {
            return $"When `{condition}`, returns `{whenTrue}`; otherwise returns `{whenFalse}`.";
        }

        if (fact.Metadata.TryGetValue("response", out var response))
        {
            return $"Returns `{response}`.";
        }

        return "Endpoint response observed, but its expression was not captured.";
    }

    private static IEnumerable<string> BuildSemanticSideEffects(FeatureCandidate candidate)
    {
        foreach (var relation in candidate.Relations.Where(relation => relation.Kind == "invokes"))
        {
            if (string.Equals(relation.Target, AspNetSignIn, StringComparison.Ordinal))
            {
                yield return "Signs in the current HTTP context using ASP.NET authentication.";
            }
            else if (string.Equals(relation.Target, AspNetSignOut, StringComparison.Ordinal))
            {
                yield return "Signs out the current HTTP context using ASP.NET authentication.";
            }
        }
    }
}
