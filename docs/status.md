# PKC Status

Last updated: 2026-09-14

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            INTERNAL TRIAL COMPLETE / INDEPENDENT REVIEW REQUIRED
V0.4.6 business logic reconstruction             REVIEW CANDIDATE / CURRENT
V0.4.7 cross-layer PO-question readiness         NEXT / LOCKED UNTIL REVIEW
V0.5 Azure DevOps input evidence                 LOCKED
```

The review target is the current `main` HEAD. Documentation commits after the code checkpoint are review/handoff only.

Last V0.4.6 code checkpoint:

```text
cbfaf6fb9107fdb233045358a2c0e7b689a647ce
```

Do not interpret this status as an independent PASS for V0.4.5 or V0.4.6.

## Product contract now governing V0.4.x

PKC V0.4.x is not finished merely because it can enumerate endpoints, workflows, validations and mutations.

The generated portable knowledge must contain enough grounded business logic for an AI to answer Product Owner questions such as:

> When is the Mewtwo card sold and shown in the web list, and what conditions are required for it to appear?

The intended evidence chain is:

```text
source/configured values
→ business predicates and boolean semantics
→ backend selection/eligibility
→ API/result flow
→ DTO/computed transformation when relevant
→ frontend visibility/list/filter behavior
→ observable product outcome
```

If a runtime database value, remote configuration, feature flag or external-system result cannot be proved from compiled inputs, PKC must say that it is unknown rather than inventing a value.

## V0.4.5 — Jellyfin generalization candidate

Pinned repository:

```text
jellyfin/jellyfin
1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

Blind record:

```text
docs/trials/2026-09-14-v0.4.5-jellyfin.md
```

Source-cross-check companion:

```text
docs/trials/2026-09-14-v0.4.5-jellyfin-crosscheck.md
```

The initial blind artifact passed structural/parity gates but source cross-check found a generic inherited MVC controller-route blocker. Regression-first hardening produced:

```text
red regression:          204b1b3e1d16d41f5c7feee2322d2e16c316b681
red run:                 34790695916 — FAIL as expected
explicit-empty guard:    f1c62410706853c047ffbbcfcde6be9a6e458867
generic route fix:       2a9ec6c00d46ebf907d543837e6e270d85c240e1
```

Post-fix checkpoint gates:

```text
CI + PokeTrade       34791017350 — PASS
pinned Loren         34791017335 — PASS
Loren-main canary    34791017349 — PASS
pinned Jellyfin      34791017353 — PASS
```

Post-fix Jellyfin artifact:

```text
artifact id:        10328282522
artifact digest:    sha256:75b9308b922d59d28a3b310f0042312948662f015062ef45167a96261ae9f8d5
canonical Markdown: 504
facts:              42,226
relations:          194,177
endpoints:          386
route corrections:  153 endpoint metadata claims
portable parity:    PASS
source/raw leak:     0
```

Warnings W3/W4/W5 remain intentionally visible to the independent reviewer: duplicate HTTP verb extraction, feature-level rule promotion, and large-pack signal/noise. Existing W1/W2 provenance/reproducibility concerns remain follow-up warnings.

## V0.4.6 — business logic reconstruction review candidate

This increment adds generic support for business-decision evidence rather than a Mewtwo-specific answer.

### Backend predicate evidence

`CSharpBusinessPredicateEnricher` now captures common expression-bodied LINQ predicate operations such as `Where`, `Any`, `All`, `First*` and `Single*` and preserves the original boolean expression.

For list inclusion, synthesis produces a grounded rule such as:

```text
Includes items from `_cards` only when
card.IsPublished
&& card.WebEnabled
&& card.SaleStartsAt <= now
&& (card.SaleEndsAt == null || now < card.SaleEndsAt)
&& card.Stock > 0
```

The boolean expression is preserved; it is not flattened into an unordered list of fields.

### Configured object evidence

For a simple predicate-source member backed by a declarative initializer, PKC can carry concrete configured object assignments as separate `configured-object` evidence.

That allows the generated knowledge to distinguish:

```text
generic eligibility rule
vs.
concrete Mewtwo configured values
```

without claiming that runtime database or remote values are statically known.

### Frontend list flow

Angular evidence now supports the direct pattern:

```text
API result
→ subscribe(result => this.collection = result)
→ component collection
→ @for (... of collection ...)
```

The resulting endpoint candidate can therefore carry both backend eligibility and the supported frontend evidence proving that the API result feeds the visible list.

### PokeTrade Mewtwo known-answer gate

The PokeTrade benchmark now gives Mewtwo a real sale/visibility decision:

```text
Name          = Mewtwo VSTAR
IsPublished   = true
WebEnabled    = true
SaleStartsAt  = 2026-09-01T00:00:00Z
SaleEndsAt    = null
Stock         = 5
```

`GetCards()` includes a card only when:

```text
IsPublished
&& WebEnabled
&& SaleStartsAt <= now
&& (SaleEndsAt == null || now < SaleEndsAt)
&& Stock > 0
```

The cross-stack regression `PokeTradePoQuestionReadinessTests` requires compiled knowledge for `Cards Get Cards` to contain the Mewtwo configured values, the complete predicate semantics, API-result binding to the frontend `cards` collection, and list rendering from that collection.

Primary regression files:

```text
tests/Pkc.CSharp.Tests/BusinessPredicateKnowledgeTests.cs
tests/Pkc.Frontend.Tests/AngularListBehaviorTests.cs
tests/Pkc.Frontend.Tests/PokeTradePoQuestionReadinessTests.cs
```

Production files:

```text
src/Pkc.CSharp/CSharpBusinessPredicateEnricher.cs
src/Pkc.CSharp/CSharpEvidenceScanner.cs
src/Pkc.Frontend/AngularListBehaviorScanner.cs
src/Pkc.Frontend/AngularFrontendAdapter.cs
src/Pkc.Knowledge/FeatureCandidateBuilder.cs
src/Pkc.Knowledge/CrossStackFeatureCandidateBuilder.cs
src/Pkc.Knowledge/GroundedKnowledgeSynthesizer.cs
```

### Code-checkpoint verification

For code checkpoint `cbfaf6fb9107fdb233045358a2c0e7b689a647ce`:

```text
CI workflow run:     34805025453 — PASS
PKC build:           PASS
PKC tests:           PASS
local tool pack:     PASS
WorkPlay build:      PASS
PokeTrade backend:   PASS
PokeTrade Angular:   PASS
PokeTrade live API:  PASS
PokeTrade knowledge: PASS
```

Exact final review-HEAD results must still be checked after the documentation-only review commits settle.

## Current V0.4.6 boundaries

The review candidate deliberately does not claim arbitrary-program business-logic reconstruction.

Current boundaries:

- expression-bodied common LINQ predicates are supported; complex block-bodied predicate lambdas need later hardening;
- configured-object reconstruction is strongest for declarative initializers behind simple source members;
- runtime DB/config/external state remains unknown unless another grounded input supplies it;
- direct Angular subscription-result assignment and `@for` list rendering are supported, but stores, RxJS transformation chains, nested frontend filters and richer visibility conditions require V0.4.7 work;
- DTO/projection/computed transformations that alter observable eligibility still need broader cross-layer coverage;
- feature-level promotion can still require quality work so important workflow rules are not buried.

These limits are part of the review contract, not hidden backlog.

## Review request

Independent review request:

```text
docs/reviews/2026-09-14-v0.4.6-business-logic-review-request.md
```

The reviewer is explicitly asked to judge both the pending V0.4.5 independent generalization gate and this V0.4.6 business-logic increment.

No coding-thread self-review may convert either milestone to PASS.

## Next after review

If the reviewer returns PASS, move to V0.4.7 and broaden cross-layer PO-question readiness: frontend visibility/filter predicates, DTO/projection transformations, richer state/data flow, observable outcome composition and high-signal rule promotion.

Do not start Azure DevOps yet.

## Azure DevOps scope lock

V0.5 Azure DevOps is planned as an additional **input evidence source** for requirement/product intent around code: Epic/Feature/PBI, acceptance intent, work-item history/status and links through PRs/commits where possible.

Its role is to help developers and AI understand why code exists and what requirement/context it implements. It is not merely delivery reporting, and it must not compensate for missing code-derived business logic.

V0.5 remains locked until the V0.4.x PO-question-readiness exit gate independently passes.

## Documentation precedence note

The final `Current next action` footer in `docs/real-project-trial.md` still reflects an older V0.4.4 execution snapshot. This `status.md`, `handoff.md` and `milestones.md` supersede that stale footer; the trial rubric itself remains valid.
