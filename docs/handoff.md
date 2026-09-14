# PKC Handoff

Use this file when continuing PKC in another coding or review thread.

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
V0.4.6  business logic reconstruction         IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING
V0.4.7  cross-layer PO-question readiness     LOCKED
V0.5    Azure DevOps input evidence           LOCKED
```

Current implementation checkpoint:

```text
34c3bf00233ac4c0c4df717f7f7c5ae55fffe07b
fix: preserve observable predicate return semantics
```

The docs-only handoff commit may be newer than that SHA. Review V0.4.6 code behavior at the implementation checkpoint above; the handoff/status update itself does not change production code.

Latest failed independent review:

```text
docs/reviews/2026-09-14-v0.4.6-independent-rereview.md
reviewed code HEAD: 45b2d9b6e215f26c395843c77f1e59887774e21b
```

Updated review request:

```text
docs/reviews/2026-09-14-v0.4.6-independent-rereview-request.md
```

## Read first in the next thread

```text
1. docs/status.md
2. docs/handoff.md
3. docs/reviews/2026-09-14-v0.4.6-independent-rereview-request.md
4. docs/reviews/2026-09-14-v0.4.6-independent-rereview.md
5. docs/milestones.md
```

Then inspect current remote `main` before changing anything.

## Accepted baseline

V0.4.4 and V0.4.5 are independently accepted PASS.

V0.4.5 pinned benchmark remains:

```text
jellyfin/jellyfin
1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

B6.2 is independently accepted PASS and should remain closed unless a new concrete contradiction appears.

## V0.4.6 blocker implementation state

```text
B6.1  IMPLEMENTED + GREEN / independent re-review pending
B6.2  PASS / keep closed
B6.3  IMPLEMENTED + GREEN / independent re-review pending
B6.4  IMPLEMENTED + GREEN / independent re-review pending
```

Do not translate the lines above into milestone PASS. V0.4.6 still needs an independent adversarial verdict.

## B6.1 — exact invocation authority

Problem found by independent review:

```text
same source line
+ custom method named Where
+ genuine LINQ Where
→ line-range authority could be borrowed by the custom invocation
```

Focused regression:

```text
Same_line_custom_Where_cannot_borrow_real_Linq_Where_semantic_authority
```

Current production authority path re-resolves the exact predicate invocation from syntax span start in the project semantic model and accepts only exact supported LINQ symbols. Failure to prove the exact invocation rejects authoritative predicate evidence.

Regression-first evidence:

```text
RED commit   cf7645eef7b22d82803ea9f48e7575677b8d2a07
RED run      34813594610 — FAIL expected
FIX commit   d784013b54acbec22dc9c18a3cfa738b38adc3d3
GREEN run    34813887033 — PASS
```

Independent reviewer should still challenge same-line collisions and all supported operation names, not only `Where`.

## B6.2 — accepted PASS

Configured-item ownership remains conservative for the reviewed forms. Nested object initializers are not promoted as direct collection items.

No further work unless a new concrete counterexample appears.

## B6.3 — deterministic Angular service identity

Problem found by independent review:

```text
different TypeScript modules
+ same exported service class name
+ same API method name
→ simple-name matching could cross-link an unrelated endpoint to a component list
```

Focused regression:

```text
Same_service_class_name_in_different_modules_does_not_cross_link_list_flow
```

Current behavior:

```text
ui-api-call owner identity:
  normalized source module path + exported owner class

ui-result-binding service identity:
  resolved relative import module + exported class

named import aliases:
  normalized to exported class identity

correlation:
  requires exact deterministic identity equality
```

If deterministic relative-module/import identity cannot be resolved, PKC does not create the authoritative endpoint-to-list match.

Regression-first evidence:

```text
RED commit   df1ea834b970201b511c8ff9a975f4e1fe59c53d
RED run      34814066361 — FAIL expected
FIX commit   84da527e8341918b1803e36ccd177a4d14056cd8
GREEN run    34814396514 — PASS
```

Independent reviewer should challenge same exported class names, same method names, nested relative paths, `index.ts` resolution and named import aliases where supported.

## B6.4 — observable predicate authority and polarity

Problem found by independent review:

A real supported LINQ predicate was previously promoted into Product Owner business wording merely because the operation existed and was reachable. That was too strong when its result did not affect the endpoint's observable result.

Canonical false-claim shape:

```csharp
public IReadOnlyList<Card> GetCards()
{
    var published = _cards.Where(card => card.IsPublished).ToArray();
    Audit(published.Length);
    return _cards;
}
```

PKC must keep the local predicate as evidence but must not claim:

```text
Includes items from `_cards` only when `card.IsPublished`.
```

Current production behavior:

```text
1. Re-resolve exact LINQ invocation semantically.
2. Keep valid local predicate evidence.
3. If direct deterministic return/yield-return participation is not proven:
   businessRuleAuthority      = observed-only
   observableEffectResolution = not-proven
   contains-condition         → observes-predicate
4. If direct return/yield-return participation is proven:
   businessRuleAuthority      = observable
   observableContext          = return / yield-return
   observableEffectResolution = direct-return-syntax
5. Synthesis only turns `contains-condition` business predicates into product-level rules.
```

Polarity/context behavior now protects the reviewed forms:

```text
Any direct boolean return:
  Returns whether at least one item ...

All direct boolean return:
  Returns whether every item ...

Where feeding the returned collection:
  Includes items ... only when ...

Any used only inside a rejecting guard:
  does not become a positive existence requirement;
  the ordinary condition/throw evidence still describes rejection behavior.
```

Focused regressions:

```text
Local_query_predicate_that_does_not_affect_return_is_not_an_authoritative_rule
Any_guard_that_throws_is_not_rendered_as_a_positive_existence_requirement
Returned_filter_predicate_remains_authoritative
Direct_any_return_is_observable_but_not_rendered_as_a_requirement
Direct_all_return_is_observable_but_not_rendered_as_a_requirement
```

Regression/fix evidence:

```text
observable-authority RED run  34817523642 — FAIL expected
observable filter fix         304a39053b243bceb6c39d85f47cc7fab68fd57f
polarity RED commit           39079f0b7a8f9abccc850f3464202f57f9caa220
polarity RED run              34817972557 — 2 FAIL expected
final B6.4 implementation     34c3bf00233ac4c0c4df717f7f7c5ae55fffe07b
```

Independent reviewer should challenge non-observable side paths, nested/local functions, Any/All polarity, direct selection operations and any path that could turn an observed predicate into a stronger endpoint claim than deterministic evidence proves.

## Full implementation-checkpoint gates

All current automation is green for `34c3bf00233ac4c0c4df717f7f7c5ae55fffe07b`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   34841796973 — PASS
pinned Loren                                34841796981 — PASS
Loren-main canary                           34841796980 — PASS
pinned Jellyfin                             34841797032 — PASS
Jellyfin portable parity/provenance         PASS
```

CI job detail:

```text
test                  PASS
poketrade-real-system PASS
```

Jellyfin job detail:

```text
build pinned source                         PASS
compile product knowledge with PKC          PASS
portable handoff parity/provenance          PASS
upload blind-review/source-cross-check pack PASS
```

Current Jellyfin artifact:

```text
artifact id      10346636986
artifact digest  sha256:5a643ec5d2ed1f84af2a70d2b0cd108a2ca13500b6c064f8a615d4481f49b987
```

## Exact next action

**Do not write more production code unless independent review finds a concrete remaining blocker.**

The next thread should perform an independent adversarial V0.4.6 re-review of B6.1, B6.3 and B6.4 against implementation checkpoint:

```text
34c3bf00233ac4c0c4df717f7f7c5ae55fffe07b
```

Review source + focused regressions + real-system output. Green automation is supporting evidence, not semantic proof.

If review returns PASS:

```text
record independent V0.4.6 PASS
update status/handoff
only then unlock V0.4.7
```

If review returns a blocker:

```text
reproduce concrete contradiction locally with one focused regression
→ generic fix
→ focused + relevant full local validation
→ one coherent commit/push
→ rerun final gates
→ independent re-review again
```

## Scope locks

Do not start V0.4.7 before independent V0.4.6 PASS.
Do not start Azure DevOps ingestion.

## Copy/paste bootstrap for an independent review thread

```text
Independently re-review PKC V0.4.6 at implementation checkpoint
34c3bf00233ac4c0c4df717f7f7c5ae55fffe07b.

Read:
1. docs/status.md
2. docs/handoff.md
3. docs/reviews/2026-09-14-v0.4.6-independent-rereview-request.md
4. docs/reviews/2026-09-14-v0.4.6-independent-rereview.md

Re-review B6.1, B6.3 and B6.4 adversarially. B6.2 is already accepted PASS.
Do not trust green automation alone; inspect implementation and construct counterexamples.
Return PASS only if no blocker-class false-product-claim path remains in the accepted V0.4.6 scope.
Do not start V0.4.7 or Azure DevOps.
```
