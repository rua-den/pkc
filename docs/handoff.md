# PKC Handoff

Use this file when continuing PKC in another coding/review thread.

## Product contract

PKC is a Product/System Knowledge Compiler. A Product Owner should be able to hand generated portable knowledge to an AI assistant and ask practical product/system questions without requiring the assistant to re-read or grep source code.

Keep the compiler architecture deterministic:

```text
source inputs
→ analyzers/adapters
→ evidence/facts
→ feature/workflow/business-decision candidates
→ knowledge synthesis
→ canonical model
→ portable rendering
```

Do not implement direct source-to-freeform-AI generation.

The V0.4.x exit standard is now explicitly business-logic/PO-question readiness, not merely endpoint or workflow enumeration.

A representative target question is:

> When is the Mewtwo card sold and shown in the web list, and what conditions are required for it to appear?

Generated knowledge must preserve enough grounded predicate, configured-value, data-flow and frontend behavior evidence for an AI to answer that class of question truthfully.

## Current state

```text
V0.4.4  Loren knowledge readiness              PASS / COMPLETE
V0.4.5  Jellyfin generalization               INTERNAL TRIAL COMPLETE / INDEPENDENT REVIEW REQUIRED
V0.4.6  business logic reconstruction         REVIEW CANDIDATE / CURRENT
V0.4.7  cross-layer PO-question readiness     NEXT / LOCKED UNTIL REVIEW
V0.5    Azure DevOps input evidence           LOCKED
```

The last code checkpoint before review-documentation commits is:

```text
cbfaf6fb9107fdb233045358a2c0e7b689a647ce
```

Review current `main` HEAD. Do not self-declare V0.4.5 or V0.4.6 PASS.

## Read first

```text
1. docs/status.md
2. docs/handoff.md
3. docs/milestones.md
4. docs/real-project-trial.md
5. docs/trials/2026-09-14-v0.4.5-jellyfin.md
6. docs/trials/2026-09-14-v0.4.5-jellyfin-crosscheck.md
7. docs/reviews/2026-09-14-v0.4.6-business-logic-review-request.md
```

For the previously accepted baseline also read, when needed:

```text
docs/reviews/2026-09-14-v0.4.4-external-rereview-4.md
```

The final `Current next action` footer in `docs/real-project-trial.md` is an older V0.4.4 snapshot. The current status/handoff/milestones supersede only that stale footer; the trial methodology/rubric remains applicable.

## V0.4.4 accepted baseline

V0.4.4 passed independent external review. Do not reopen it without new contradictory evidence.

Accepted review:

```text
docs/reviews/2026-09-14-v0.4.4-external-rereview-4.md
```

Important invariant retained from V0.4.4:

```text
No deterministic proof → do not promote cross-stack semantic equivalence to a high-confidence consistent claim.
```

## V0.4.5 Jellyfin state

Pinned benchmark:

```text
repository: jellyfin/jellyfin
commit:     1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

Blind review was frozen before source inspection. Source cross-check then found a real generic MVC route-inheritance blocker.

Regression/fix chain:

```text
red regression:       204b1b3e1d16d41f5c7feee2322d2e16c316b681
red run:              34790695916 — FAIL as expected
override protection:  f1c62410706853c047ffbbcfcde6be9a6e458867
generic fix:          2a9ec6c00d46ebf907d543837e6e270d85c240e1
```

Post-fix checkpoint:

```text
CI + PokeTrade       34791017350 — PASS
pinned Loren         34791017335 — PASS
Loren-main canary    34791017349 — PASS
pinned Jellyfin      34791017353 — PASS
```

The corrected Jellyfin artifact retained exact portable parity and no source leak and corrected 153 endpoint route metadata claims.

Independent review must still classify warnings W3/W4/W5 and decide V0.4.5 PASS or blocker.

## V0.4.6 implementation

### C# business predicate reconstruction

Production:

```text
src/Pkc.CSharp/CSharpBusinessPredicateEnricher.cs
src/Pkc.CSharp/CSharpEvidenceScanner.cs
```

Regression:

```text
tests/Pkc.CSharp.Tests/BusinessPredicateKnowledgeTests.cs
```

Supported common expression-lambda operations include `Where`, `Any`, `All`, `First*` and `Single*`.

The raw boolean expression is preserved so knowledge can distinguish logic such as:

```text
A && B && (C || D) && E
```

from an unordered bag of referenced fields.

### Configured instance evidence

For a simple predicate-source member backed by declarative object initializers, the C# enricher emits separate `configured-object` evidence. This allows a generic eligibility rule to be combined with a concrete statically configured instance without pretending the configured value is itself a predicate.

Do not generalize this to runtime DB/config/external values unless a future evidence source grounds those values.

### Angular list result flow

Production:

```text
src/Pkc.Frontend/AngularListBehaviorScanner.cs
src/Pkc.Frontend/AngularFrontendAdapter.cs
```

Regression:

```text
tests/Pkc.Frontend.Tests/AngularListBehaviorTests.cs
```

Supported direct pattern:

```text
api.getCards()
→ subscribe(cards => this.cards = cards)
→ @for (card of cards; ...)
```

Facts:

```text
ui-result-binding
ui-list-render
feeds-list
```

Confidence/provenance is intentionally syntactic/structural rather than falsely claiming full TypeScript type-checker semantics.

### Cross-stack knowledge

Production:

```text
src/Pkc.Knowledge/FeatureCandidateBuilder.cs
src/Pkc.Knowledge/CrossStackFeatureCandidateBuilder.cs
src/Pkc.Knowledge/GroundedKnowledgeSynthesizer.cs
```

Endpoint knowledge can now carry the relevant predicate, configured data, matching frontend API call, result binding and rendered list for the supported pattern.

## PokeTrade PO known-answer gate

The PokeTrade benchmark now contains a real Mewtwo sale/visibility rule.

Mewtwo static configuration:

```text
Name          = Mewtwo VSTAR
IsPublished   = true
WebEnabled    = true
SaleStartsAt  = 2026-09-01T00:00:00Z
SaleEndsAt    = null
Stock         = 5
```

Backend inclusion predicate:

```text
card.IsPublished
&& card.WebEnabled
&& card.SaleStartsAt <= now
&& (card.SaleEndsAt == null || now < card.SaleEndsAt)
&& card.Stock > 0
```

Frontend supported proof:

```text
GET /api/cards
→ getCards result
→ CatalogComponent.cards
→ @for card of cards
```

End-to-end regression:

```text
tests/Pkc.Frontend.Tests/PokeTradePoQuestionReadinessTests.cs
```

The test compiles real PokeTrade backend + frontend evidence and requires synthesized knowledge to contain both the Mewtwo values and the cross-layer decision chain.

## Code-checkpoint verification

For code checkpoint `cbfaf6fb9107fdb233045358a2c0e7b689a647ce`:

```text
CI run              34805025453 — PASS
dotnet build        PASS
dotnet test         PASS
tool pack/install   PASS
WorkPlay build      PASS
PokeTrade backend   PASS
PokeTrade Angular   PASS
live API acceptance PASS
knowledge compile   PASS
```

After the final docs commit, review exact current HEAD and verify all four workflow families again:

```text
CI / PokeTrade
pinned Loren
Loren-main canary
pinned Jellyfin
```

## Boundaries reviewer must challenge

Current known boundaries:

- block-bodied/complex query predicates are not yet generally reconstructed;
- configured-object evidence is strongest for simple source members with declarative initializers;
- DB/runtime/remote configuration remains unknown without grounded input;
- Angular direct subscription assignment + `@for` is covered, but stores, RxJS transformations, nested filters and richer visibility conditions remain V0.4.7 work;
- DTO/projection/computed transformations that affect observable eligibility need broader coverage;
- feature-level promotion can still bury useful workflow rules;
- Jellyfin W3/W4/W5 and existing W1/W2 remain review concerns.

Any generated knowledge that claims certainty beyond these boundaries should be treated as a blocker.

## Azure DevOps scope

Do not implement Azure DevOps yet.

When V0.5 eventually unlocks, ADO is an additional **input evidence source** for requirement/product intent around code: Epic/Feature/PBI, acceptance intent, history/status and links through PRs/commits where possible.

The purpose is to help developers and AI understand what requirement/context the code implements and why it exists. It is not only delivery reporting.

ADO must not be used to hide a failure to reconstruct the business logic that is observable in code.

## Independent review bootstrap

```text
Continue PKC review from current main HEAD.

Read in order:
1. docs/status.md
2. docs/handoff.md
3. docs/milestones.md
4. docs/real-project-trial.md
5. docs/trials/2026-09-14-v0.4.5-jellyfin.md
6. docs/trials/2026-09-14-v0.4.5-jellyfin-crosscheck.md
7. docs/reviews/2026-09-14-v0.4.6-business-logic-review-request.md

Review both pending gates:
- V0.4.5 Jellyfin independent real-repository generalization
- V0.4.6 business logic reconstruction

Challenge the inherited MVC route fix, no-hardcoding/generalization, C# predicate AND/OR preservation, configured-object evidence, Angular API-result-to-list flow, the real PokeTrade Mewtwo PO-question regression, known limitations, and all cross-benchmark gates.

Return PASS only if there is no unresolved blocker. Otherwise give concrete blockers with source/evidence references.

Do not start V0.4.7 or V0.5 during this review.
```
