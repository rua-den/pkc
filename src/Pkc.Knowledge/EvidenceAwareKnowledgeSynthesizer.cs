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
        var sideEffects = knowledge.SideEffects
            .Concat(BuildSemanticSideEffects(candidate))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return knowledge with { SideEffects = sideEffects };
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
