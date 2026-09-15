# PKC Status

Last updated: 2026-09-15

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING
V0.4.7 cross-layer PO-question readiness         LOCKED
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.6 is **not complete yet**. The two blockers from the latest independent review have been fixed with adversarial regression coverage and the full exact-checkpoint gate set is green, but milestone acceptance still requires a new independent adversarial re-review.

Current implementation checkpoint:

```text
9a0817b21075a3d810072a310ed8fd3314625cd8
fix: enforce observable authority boundaries
```

Latest failed independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-2.md
reviewed implementation: 34c3bf00233ac4c0c4df717f7f7c5ae55fffe07b
```

V0.4.3 remains the last accepted tool package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

## Product contract governing V0.4.x

PKC must generate portable knowledge rich enough for an AI to answer practical Product Owner questions about observable behavior, business conditions and cross-layer outcomes without re-reading source code.

Representative question:

> When is entity X sellable/visible on the web, and what exact conditions must be true for it to appear?

Required evidence chain where source can prove it:

```text
configured/static values
→ business predicates and boolean semantics
→ backend selection/eligibility
→ API/result flow
→ DTO/computed transformation when relevant
→ frontend visibility/list/filter behavior
→ observable product outcome
```

No deterministic proof means no authoritative product claim. Runtime database/config/external values remain unknown unless grounded by another input.

## V0.4.6 blocker state

### B6.1 — PASS / keep closed

Exact C# predicate authority is re-resolved from the exact invocation syntax `SpanStart` in the target project semantic model. Same-line custom and genuine LINQ invocations cannot share semantic authority merely because they occupy the same source line/range.

Focused regression remains:

```text
Same_line_custom_Where_cannot_borrow_real_Linq_Where_semantic_authority
```

Do not reopen B6.1 without a new concrete contradiction.

### B6.2 — PASS / keep closed

Configured-item ownership remains conservative for the reviewed forms. Nested property object initializers are not promoted as additional direct collection items.

Do not reopen B6.2 without a new concrete contradiction.

### B6.3 — IMPLEMENTED + GREEN / independent re-review pending

Previous blocker: Angular service ownership could be overwritten by import-like text inside comments/strings/templates because the fallback import matcher scanned raw source text.

Current implementation property:

```text
module-qualified owner identity is preserved
only lexically active import declarations may establish service ownership
line comments / block comments / strings / template literals have zero import authority
conflicting active local-name imports are treated as ambiguous and omitted
unresolved relative module identity remains uncorrelated
```

Focused adversarial regression:

```text
Inactive_import_like_text_does_not_override_active_service_module
```

The fixture includes line-comment, block-comment, ordinary-string and template-literal import-like collisions while two modules export the same `CardsApi.getCards()` identity shape.

### B6.4 — IMPLEMENTED + GREEN / independent re-review pending

Previous blocker: merely being syntactically contained somewhere inside a return expression could promote a LINQ predicate to observable product behavior, even when surrounding syntax inverted polarity, transformed the result type, or an arbitrary helper discarded the value.

Current authority property:

```text
supported predicate invocation
+ exact LINQ semantic identity
+ conservatively proven value/polarity-preserving path to return/yield-return
→ authoritative Product Owner rule

otherwise
→ businessRuleAuthority=observed-only
→ observes-predicate
→ no product-level rule promotion
```

Current supported positive authority paths include:

```text
Any / All / First* / Single*  → direct returned value, parentheses allowed
Where                          → direct return or semantically resolved allowlisted LINQ/materialization receiver chain
```

Arbitrary helper wrapping is not considered proof.

Focused regressions now cover:

```text
!Any(...)
!All(...)
FirstOrDefault(...) is null
SingleOrDefault(...) is not null
Where passed through an arbitrary helper that discards the result
positive direct Any/All/First returns
positive returned Where
positive Where → Select → ToArray pipeline
throwing Any guard
local discarded Where
```

Observable promoted facts now record:

```text
observableEffectResolution=semantic-return-value-path
```

## Exact implementation-checkpoint automation

All push-triggered gates for implementation checkpoint `9a0817b21075a3d810072a310ed8fd3314625cd8` are green:

```text
CI + PKC tests + WorkPlay + PokeTrade   34920522723 — PASS
pinned Loren                            34920522961 — PASS
Loren-main canary                       34920522799 — PASS
pinned Jellyfin                         34920522831 — PASS
portable parity / provenance / no-leak  PASS inside Jellyfin run
```

Exact CI evidence:

```text
PKC build:       0 warnings / 0 errors
C# tests:        72 / 72 PASS
frontend tests:  13 / 13 PASS
tool pack/install: PASS
WorkPlay:          PASS
PokeTrade backend: PASS
PokeTrade Angular: PASS
PokeTrade live business branches: PASS
PokeTrade generated-knowledge assertions: PASS
```

Real-repository gates:

```text
pinned Loren build + PKC compile:        PASS
current Loren-main build + PKC compile:  PASS
pinned Jellyfin source build:            PASS, 0 warnings / 0 errors
Jellyfin PKC facts:                      43,363
Jellyfin workflow candidates:            386
Jellyfin product features:               116
canonical Markdown files:                504
analysis mode:                           project-semantic 43,363 / 43,363
```

Portable Jellyfin verification:

```text
every canonical Markdown file appears verbatim in PKC_KNOWLEDGE.md: PASS
ZIP file set equals canonical Markdown file set:                       PASS
ZIP bytes equal canonical Markdown bytes:                             PASS
raw .pkc leak in portable ZIP:                                        none
src/ source-tree leak in portable ZIP:                                none
```

Current Jellyfin artifact:

```text
artifact id:     10377409728
artifact digest: sha256:74ded0b0e5271b1731599b3490d7013ad07dde74445e893868320e2e657f714d
```

## Benchmark-special-case check

The B6.3/B6.4 production changes are generic analyzer/authority logic. No PokeTrade, Mewtwo, Loren or Jellyfin-specific production exception was added. Benchmark names remain only where appropriate in tests, samples and acceptance material.

## Repository-history note

A transient accidental placeholder commit exists immediately before the implementation checkpoint:

```text
57831e0541100cd7d629283f0c8a57bc8b0c0363
```

The implementation tree at `9a0817b21075a3d810072a310ed8fd3314625cd8` removes that placeholder. The clean baseline-to-implementation diff contains only the intended two production files and three regression-test files.

## Exact next action

Stay in V0.4.6 and perform **independent adversarial re-review only** against implementation checkpoint:

```text
9a0817b21075a3d810072a310ed8fd3314625cd8
```

The reviewer must inspect implementation and adversarial regressions rather than accepting green CI as semantic proof.

Challenge B6.3 with active/inactive import collisions, aliases, nested/index modules, conflicting imports and unresolved ownership. Challenge B6.4 with polarity/result transformations, arbitrary helper boundaries, positive direct returns, safe returned LINQ pipelines and guard/throw contexts.

If the independent review returns PASS:

```text
mark V0.4.6 COMPLETE
unlock V0.4.7 as next/current milestone
keep V0.5 Azure DevOps locked
```

If it finds a concrete blocker:

```text
stay in V0.4.6
record the counterexample
fix only that blocker regression-first
```

Do not start V0.4.7 before independent V0.4.6 PASS.
Do not start Azure DevOps ingestion.
