# PKC Handoff

Use this file when continuing PKC in another coding/review thread.

## Product contract

PKC is a deterministic Product/System Knowledge Compiler. A Product Owner should be able to hand generated portable knowledge to an AI and ask practical product/system questions without the AI re-reading source code.

Keep the architecture:

```text
source inputs
→ deterministic analyzers/adapters
→ evidence/facts
→ feature/workflow/business-decision candidates
→ knowledge synthesis
→ canonical model
→ portable rendering
```

Do not add direct source-to-freeform-AI generation.

The V0.4.x exit standard is business-logic and PO-question readiness, not merely endpoint/workflow enumeration.

## Current state

```text
V0.4.4  Loren knowledge readiness              PASS / COMPLETE
V0.4.5  Jellyfin generalization               PASS / COMPLETE
V0.4.6  business logic reconstruction         INDEPENDENT REVIEW FAILED / FIX REQUIRED
V0.4.7  cross-layer PO-question readiness     LOCKED
V0.5    Azure DevOps input evidence           LOCKED
```

Independent review record:

```text
docs/reviews/2026-09-14-v0.4.5-v0.4.6-independent-review.md
```

Reviewed HEAD:

```text
c293fc157783ce416af9e5730b6b4de005b26b6b
```

Review-document commits after that HEAD do not change the reviewed implementation.

## Read first

```text
1. docs/status.md
2. docs/handoff.md
3. docs/milestones.md
4. docs/reviews/2026-09-14-v0.4.5-v0.4.6-independent-review.md
5. docs/trials/2026-09-14-v0.4.5-jellyfin.md
6. docs/trials/2026-09-14-v0.4.5-jellyfin-crosscheck.md
7. docs/reviews/2026-09-14-v0.4.6-business-logic-review-request.md
```

## Accepted baselines

### V0.4.4

Passed independent external review. Keep the conservative semantic-authority rule:

```text
No deterministic proof → no high-confidence semantic equivalence/claim.
```

### V0.4.5

Jellyfin independent real-repository generalization is accepted.

Pinned benchmark:

```text
jellyfin/jellyfin
1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

The inherited MVC controller-route issue was found during source cross-check, reproduced red first and fixed generically. Explicit `[Route("")]` override behavior is protected. Exact reviewed-head PokeTrade/Loren/Jellyfin runs are green.

Keep warnings W3/W4/W5 visible, but do not reopen V0.4.5 unless new contradictory evidence appears or a V0.4.6 fix regresses Jellyfin.

## V0.4.6 blocker set

There are exactly three independent-review blockers at this handoff.

### B6.1 — prove business-predicate operation semantics

Affected production:

```text
src/Pkc.CSharp/CSharpBusinessPredicateEnricher.cs
src/Pkc.CSharp/CSharpEvidenceScanner.cs
```

Current problem:

```text
method name is Where/Any/All/First*/Single*
+ expression lambda
→ treated as authoritative business predicate
```

No symbol proof currently establishes that the invoked method is a supported `System.Linq` operation. The predicate facts are added after semantic enrichment and currently have no equivalent semantic-provenance guard.

Reachable false claim:

```csharp
public sealed class CustomBucket
{
    public IReadOnlyList<Card> Where(Func<Card, bool> ignored) => _allCards;
}

public IReadOnlyList<Card> GetCards()
    => _bucket.Where(card => card.IsPublished);
```

PKC must not claim `_bucket` includes only published cards unless the operation semantics are proved.

Required:

```text
red custom-Where regression
→ semantic operation proof for supported LINQ family
→ unresolved/custom methods cannot become definitive Includes/Requires/Selects rules
```

No lexical or repository-specific allowlist workaround.

### B6.2 — prove configured item ownership

Affected production:

```text
src/Pkc.CSharp/CSharpBusinessPredicateEnricher.cs
src/Pkc.Knowledge/GroundedKnowledgeSynthesizer.cs
```

Current code walks all descendant object creations inside the predicate-source field initializer and labels every one as a `configured-object` for that source. Synthesis then renders every one as a configured item in the collection.

Nested object creations are not collection items.

Required regression shape:

```csharp
private readonly List<Card> _cards =
[
    new Card
    {
        Name = "A",
        Metadata = new CardMetadata { Name = "Internal metadata" }
    }
];
```

`CardMetadata` must not become another `Configured item in _cards` claim.

Required:

```text
red nested-initializer regression
→ prove direct item/value ownership
→ only direct supported source items become configured-object item evidence
```

### B6.3 — prove Angular API service ownership for result/list flow

Affected production:

```text
src/Pkc.Frontend/AngularListBehaviorScanner.cs
src/Pkc.Knowledge/CrossStackFeatureCandidateBuilder.cs
```

Current correlation uses only:

```text
binding.apiMethod == apiCall.Container
```

This is insufficient because different services can expose the same method name.

Required regression shape:

```text
CatalogApi.getCards() → GET /api/cards
AdminApi.getCards()   → GET /api/admin/cards

CatalogComponent uses catalogApi.getCards()
→ cards
→ @for card of cards
```

The `/api/admin/cards` endpoint must never inherit the `CatalogComponent.cards` result/list flow solely because its service method is also named `getCards`.

Required:

```text
red ambiguous-service regression
→ service/class/import/injection ownership proof or another conservative ownership key
→ method-name equality alone never proves endpoint-to-list flow
```

If ownership cannot be proved, omit the cross-stack list claim.

## V0.4.6 parts to preserve

Do not regress these while fixing B6.1–B6.3:

- complete raw boolean expression and AND/OR/parentheses are preserved for supported predicate evidence;
- PokeTrade Mewtwo known-answer values and rule remain answerable for the supported case;
- direct Angular subscribe-result assignment and `@for` rendering stay explicitly medium-confidence syntactic evidence;
- PokeTrade, pinned Loren, Loren-main and pinned Jellyfin stay green;
- portable parity/no-leak behavior stays intact;
- no PokeTrade/Mewtwo/Jellyfin production special cases.

## Reviewed exact-head gates

On `c293fc157783ce416af9e5730b6b4de005b26b6b`:

```text
CI / PokeTrade             34805328962 — PASS
pinned Loren               34805329030 — PASS
Loren-main canary          34805329006 — PASS
pinned Jellyfin            34805328945 — PASS
```

These gates do not prove away B6.1–B6.3; current fixtures simply do not contain those adversarial shapes.

## Required coding sequence

Stay in V0.4.6 and work regression-first.

Recommended order:

```text
B6.1 semantic business-predicate proof
B6.2 configured-item ownership
B6.3 Angular service-aware result flow
```

For every blocker:

```text
commit red focused regression
→ verify expected failure
→ generic fix commit
→ focused regression green
```

After all three fixes, rerun:

```text
full PKC tests
PokeTrade known-answer + live acceptance
pinned Loren
Loren-main canary
pinned Jellyfin
portable parity / no source leak
```

Then update `docs/status.md` and `docs/handoff.md` with exact runs and request independent V0.4.6 re-review.

Do not start V0.4.7 or V0.5 Azure DevOps before that review passes.

## Coding-thread bootstrap

```text
Continue PKC from current main HEAD.

Read docs/status.md, docs/handoff.md and docs/reviews/2026-09-14-v0.4.5-v0.4.6-independent-review.md.

V0.4.5 is accepted PASS. Fix only V0.4.6 blockers B6.1, B6.2 and B6.3, regression-first and generically. Do not start V0.4.7 or Azure DevOps. Preserve PokeTrade + Loren + Jellyfin gates and portable parity, then request independent V0.4.6 re-review.
```
