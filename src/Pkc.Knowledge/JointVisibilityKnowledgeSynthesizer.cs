using Pkc.Core;

namespace Pkc.Knowledge;

public sealed class JointVisibilityKnowledgeSynthesizer : IKnowledgeSynthesizer
{
    private readonly IKnowledgeSynthesizer _inner;

    public JointVisibilityKnowledgeSynthesizer(IKnowledgeSynthesizer? inner = null)
    {
        _inner = inner ?? new EvidenceAwareKnowledgeSynthesizer();
    }

    public async ValueTask<FeatureKnowledge> SynthesizeAsync(
        FeatureCandidate candidate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        var knowledge = await _inner.SynthesizeAsync(candidate, cancellationToken);
        var visibilityFacts = candidate.Facts
            .Where(fact => fact.Kind == "ui-member-visibility")
            .OrderBy(fact => fact.Source.Path, StringComparer.Ordinal)
            .ThenBy(fact => fact.Source.StartLine)
            .ToArray();
        var jointFacts = candidate.Facts
            .Where(fact => fact.Kind == "joint-visibility")
            .OrderBy(fact => fact.Source.Path, StringComparer.Ordinal)
            .ThenBy(fact => fact.Source.StartLine)
            .ToArray();

        var rules = knowledge.Rules
            .Concat(visibilityFacts.Select(DescribeFrontendVisibilityRule))
            .Concat(jointFacts
                .Where(fact => string.Equals(
                    fact.Metadata.GetValueOrDefault("jointVisibilityAuthority"),
                    "observable",
                    StringComparison.Ordinal))
                .Select(DescribeJointVisibilityRule))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var unknowns = knowledge.Unknowns
            .Concat(jointFacts
                .Where(fact => !string.Equals(
                    fact.Metadata.GetValueOrDefault("jointVisibilityAuthority"),
                    "observable",
                    StringComparison.Ordinal))
                .Select(DescribeAuthorityLimitation))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var evidence = knowledge.Evidence
            .Concat(visibilityFacts.Select(fact => new KnowledgeEvidence(
                fact.Id,
                fact.Kind,
                DescribeFrontendVisibilityEvidence(fact),
                fact.Source)))
            .Concat(jointFacts.Select(fact => new KnowledgeEvidence(
                fact.Id,
                fact.Kind,
                DescribeJointVisibilityEvidence(fact),
                fact.Source)))
            .GroupBy(item => item.FactId, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => item.Source.Path, StringComparer.Ordinal)
            .ThenBy(item => item.Source.StartLine)
            .ToArray();

        return knowledge with
        {
            Rules = rules,
            Unknowns = unknowns,
            Evidence = evidence
        };
    }

    private static string DescribeFrontendVisibilityRule(EvidenceFact fact)
    {
        var component = fact.Metadata.GetValueOrDefault("component") ?? fact.Container ?? "UI";
        var member = fact.Metadata.GetValueOrDefault("member") ?? fact.Name;
        var condition = fact.Metadata.GetValueOrDefault("condition") ?? "unknown condition";
        return $"Frontend visibility for rendered `{component}.{member}`: visible when `{condition}`.";
    }

    private static string DescribeJointVisibilityRule(EvidenceFact fact)
    {
        var component = fact.Container ?? "UI";
        var member = fact.Metadata.GetValueOrDefault("frontendComponentMember") ?? fact.Name;
        var backendCondition = fact.Metadata.GetValueOrDefault("backendCondition") ?? "unknown backend condition";
        var frontendCondition = fact.Metadata.GetValueOrDefault("frontendCondition") ?? "unknown frontend condition";
        return $"Combined observable visibility for `{component}.{member}` on the same proven API → UI value path: backend selects the response item where `{backendCondition}`; frontend renders the value when `{frontendCondition}`.";
    }

    private static string DescribeAuthorityLimitation(EvidenceFact fact)
    {
        var component = fact.Container ?? "UI";
        var member = fact.Metadata.GetValueOrDefault("frontendComponentMember") ?? fact.Name;
        var backendCondition = fact.Metadata.GetValueOrDefault("backendCondition") ?? "unknown backend condition";
        var frontendCondition = fact.Metadata.GetValueOrDefault("frontendCondition") ?? "unknown frontend condition";
        return $"Joint visibility for `{component}.{member}` is not promoted to an authoritative combined rule because backend condition `{backendCondition}` is not authoritative. Frontend condition `{frontendCondition}` remains independently observed.";
    }

    private static string DescribeFrontendVisibilityEvidence(EvidenceFact fact)
    {
        var component = fact.Metadata.GetValueOrDefault("component") ?? fact.Container ?? "UI";
        var member = fact.Metadata.GetValueOrDefault("member") ?? fact.Name;
        var condition = fact.Metadata.GetValueOrDefault("condition") ?? "unknown condition";
        return $"Bounded frontend visibility: `{component}.{member}` is rendered inside `@if ({condition})`.";
    }

    private static string DescribeJointVisibilityEvidence(EvidenceFact fact)
    {
        var backendCondition = fact.Metadata.GetValueOrDefault("backendCondition") ?? "unknown";
        var backendAuthority = fact.Metadata.GetValueOrDefault("backendAuthority") ?? "unknown";
        var frontendCondition = fact.Metadata.GetValueOrDefault("frontendCondition") ?? "unknown";
        var terminal = fact.Metadata.GetValueOrDefault("r79TerminalFactId") ?? "unknown";
        return $"Joint visibility composition on R7.9 terminal `{terminal}`: backend `{backendCondition}` (authority `{backendAuthority}`) + frontend `{frontendCondition}`.";
    }
}
