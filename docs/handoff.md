# PKC Handoff

Use this file when continuing PKC in another coding or independent-review thread.

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

Current implementation checkpoint awaiting independent review:

```text
9a0817b21075a3d810072a310ed8fd3314625cd8
fix: enforce observable authority boundaries
```

Latest failed independent review that defined the coding scope:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-2.md
reviewed code: 34c3bf00233ac4c0c4df717f7f7c5ae55fffe07b
```

Current disposition before the new independent verdict:

```text
B6.1  PASS / keep closed
B6.2  PASS / keep closed
B6.3  IMPLEMENTED + GREEN / independent re-review pending
B6.4  IMPLEMENTED + GREEN / independent re-review pending
```

Do not translate implementation-green into milestone PASS. V0.4.6 still requires an independent adversarial verdict.

## Read first in the next independent review thread

```text
1. docs/status.md
2. docs/handoff.md
3. docs/milestones.md
4. docs/reviews/2026-09-15-v0.4.6-independent-rereview-2.md
5. prior V0.4.6 independent review/rereview documents as needed
```

Then inspect exact implementation checkpoint `9a0817b21075a3d810072a310ed8fd3314625cd8` directly.

## Accepted/closed scope

### B6.1 — PASS

The same-line semantic-authority collision remains closed. The final C# predicate authority stage re-resolves the exact invocation by syntax `SpanStart` in the target project semantic model and accepts only the exact supported LINQ symbol.

Regression:

```text
Same_line_custom_Where_cannot_borrow_real_Linq_Where_semantic_authority
```

Do not reopen without a new concrete contradiction.

### B6.2 — PASS

Configured-item ownership remains conservative for reviewed supported initializer forms. Nested property objects are not promoted as direct configured items of the source collection.

Do not reopen without a new concrete contradiction.

## B6.3 implementation checkpoint — active Angular import ownership

The previous independent blocker showed that raw-text import scanning could allow comments/strings/templates to overwrite a real service import and cross-link an unrelated endpoint to a rendered list.

Current generic behavior at `9a0817...`:

```text
API owner identity:
  normalized TypeScript source module path + exported class

component service identity:
  resolved relative active import module + exported class

inactive import-like text:
  line comments     → no authority
  block comments    → no authority
  quoted strings    → no authority
  template literals → no authority

conflicting active local-name imports:
  treated as ambiguous
  no deterministic service identity emitted

unresolved relative module:
  no authoritative endpoint → result/list correlation
```

Focused regression:

```text
Inactive_import_like_text_does_not_override_active_service_module
```

The adversarial fixture has catalog/admin modules exporting the same `CardsApi.getCards()` and places fake admin imports in line-comment, block-comment, string and template text. Catalog correlation must remain correct and the admin endpoint must not inherit the catalog result/list flow.

Independent reviewer should still challenge:

```text
named import aliases
nested relative module paths
index.ts resolution
multiple active bindings with same local name
comment/string/template boundaries
unresolved/non-relative imports
same class + same method name across modules
```

Conservative omission is preferable to guessed ownership.

## B6.4 implementation checkpoint — observable value/polarity authority

The previous independent blocker showed that syntactic containment anywhere in a returned expression did not prove that the predicate's value, type or polarity reached the observable result unchanged.

Current generic behavior at `9a0817...`:

```text
exact supported LINQ invocation
+ method-local return/yield-return context
+ operation-specific value/polarity-preserving path proof
→ businessRuleAuthority=observable
→ observableEffectResolution=semantic-return-value-path
→ contains-condition may become product-level rule

otherwise
→ businessRuleAuthority=observed-only
→ observableEffectResolution=not-proven
→ observes-predicate
→ no product-level rule promotion
```

Current supported positive paths:

```text
Any / All / First / FirstOrDefault / Single / SingleOrDefault:
  invocation must be the direct returned/yielded value, with parentheses allowed

Where:
  direct return, or a semantically resolved allowlisted receiver pipeline that preserves upstream filtering
```

The current returned-Where pipeline allowlist covers relevant Enumerable/Queryable projection/order/paging/distinct/reverse/as-enumerable/as-queryable operations plus Enumerable materialization (`ToArray`, `ToList`, `ToHashSet`). Every outer receiver-chain invocation must resolve to the allowed LINQ target.

Arbitrary helpers are not assumed to preserve the predicate result.

Adversarial regressions now include:

```text
Negated_any_return_is_observed_only_because_polarity_is_inverted
Negated_all_return_is_observed_only_because_polarity_is_inverted
First_or_default_null_test_is_not_rendered_as_returning_a_selected_item
Single_or_default_null_test_is_not_rendered_as_returning_a_selected_item
Filter_passed_to_helper_that_discards_result_is_not_authoritative
Returned_filter_through_supported_projection_pipeline_remains_authoritative
Direct_first_return_remains_an_observable_selection
```

Existing regressions retained for direct Any/All, direct returned Where, local discarded Where and rejecting Any guard/throw behavior.

Independent reviewer should challenge both false-positive and false-negative boundaries, especially:

```text
!Any / !All
binary/pattern/null transformations around First*/Single*
nested arbitrary helper calls
conditional/coalescing transformations
safe Where → Select/order/materialize receiver chains
direct Any/All/First/Single returns
guards + throws
nested/local functions
```

If an enclosing transformation is not deterministically modeled, lower authority/omission is the intended safe behavior.

## Exact implementation-checkpoint gates

All final gates for `9a0817b21075a3d810072a310ed8fd3314625cd8` are green:

```text
CI + PKC tests + WorkPlay + PokeTrade   34920522723 — PASS
pinned Loren                            34920522961 — PASS
Loren-main canary                       34920522799 — PASS
pinned Jellyfin                         34920522831 — PASS
Jellyfin portable parity/provenance     PASS
```

Exact CI counts/results:

```text
PKC build:                 0 warnings / 0 errors
C# tests:                  72 / 72 PASS
frontend tests:            13 / 13 PASS
local tool pack/install:   PASS
WorkPlay build:            PASS
PokeTrade backend build:   PASS
PokeTrade Angular build:   PASS
PokeTrade live branches:   PASS
PokeTrade knowledge check: PASS
```

Real-project gates:

```text
pinned Loren build + compile:       PASS
current Loren-main build + compile: PASS
pinned Jellyfin source build:       PASS, 0 warnings / 0 errors
Jellyfin PKC facts:                 43,363
Jellyfin relations:                 195,314
workflow candidates:                386
product features:                   116
canonical Markdown files:           504
analysis mode:                      project-semantic 43,363 / 43,363
```

Portable verification:

```text
canonical Markdown preserved verbatim in single-file bundle: PASS
ZIP exact file-set parity:                                  PASS
ZIP byte parity:                                            PASS
raw .pkc leak:                                              none
src/ source-tree leak:                                      none
```

Current Jellyfin artifact:

```text
artifact id:     10377409728
artifact digest: sha256:74ded0b0e5271b1731599b3490d7013ad07dde74445e893868320e2e657f714d
```

Green automation is supporting evidence only. The next reviewer must inspect the implementation independently and attempt adversarial counterexamples.

## Benchmark-special-case check

No PokeTrade, Mewtwo, Loren or Jellyfin-specific production exception was introduced in the B6.3/B6.4 implementation. Benchmark names remain in tests/samples/acceptance material only where expected.

## History note

A transient accidental placeholder commit exists in history immediately before the implementation checkpoint:

```text
57831e0541100cd7d629283f0c8a57bc8b0c0363
```

Do not review that transient tree. The implementation checkpoint `9a0817b21075a3d810072a310ed8fd3314625cd8` removes the placeholder. Clean baseline-to-checkpoint diff contains only the intended two production files and three regression files.

## Exact next action

**Do not write more production code unless the independent re-review finds a concrete remaining blocker.**

Review exact implementation checkpoint:

```text
9a0817b21075a3d810072a310ed8fd3314625cd8
```

Re-review B6.3 and B6.4 adversarially. Confirm B6.1/B6.2 remain closed.

If independent review returns PASS:

```text
record independent V0.4.6 PASS
mark V0.4.6 COMPLETE
unlock V0.4.7 as next/current milestone
keep V0.5 Azure DevOps locked
```

If review returns a blocker:

```text
record exact counterexample
stay in V0.4.6
reproduce regression-first
implement only the generic blocker fix
validate locally where possible
one coherent implementation push
rerun all gates
independent review again
```

## Scope locks

Do not start V0.4.7 before independent V0.4.6 PASS.
Do not start Azure DevOps ingestion.

## Copy/paste bootstrap for the independent review thread

```text
Independently re-review PKC V0.4.6 at implementation checkpoint
9a0817b21075a3d810072a310ed8fd3314625cd8.

Read:
1. docs/status.md
2. docs/handoff.md
3. docs/milestones.md
4. docs/reviews/2026-09-15-v0.4.6-independent-rereview-2.md

B6.1 and B6.2 are previously accepted PASS; verify they remain regression-safe.
Adversarially re-review the B6.3 and B6.4 fixes.
Do not trust green CI alone.
Return PASS only if no blocker-class false-product-claim path remains in V0.4.6 scope.
Do not implement fixes in the review thread.
Do not start V0.4.7 or Azure DevOps unless V0.4.6 independently passes.
```
