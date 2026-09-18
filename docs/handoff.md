# PKC Handoff

Use this file when continuing PKC in another coding or review thread.

## Read first

Read in this exact order before changing production code:

1. `docs/status.md`
2. this handoff
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/v0.4.7-acceptance-plan.md`
6. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md`
7. `docs/reviews/2026-09-16-v0.4.7-plan-readiness-review.md`
8. `docs/reviews/2026-09-16-v0.4.7-snapshot-checkpoint-review.md`
9. `docs/reviews/2026-09-17-v0.4.7-alias-composition-review.md`
10. `docs/reviews/2026-09-17-v0.4.7-reference-dynamic-delivery.md`
11. `docs/reviews/2026-09-17-v0.4.7-b-computation-causality-delivery.md`
12. `docs/reviews/2026-09-18-v0.4.7-b-independent-review.md`

Then inspect `git status`, current `main` HEAD and recent commits. Never assume or reset to an older SHA if `main` has advanced.

## Current main checkpoint

At this handoff the latest verified production-code checkpoint on `main` is:

```text
e5b0d47c82b6db99f5184292730919421a6d2a06
feat: prove V0.4.7-B computation and causality
```

This SHA passed its implementation gates, but independent review found a stale-terminal-source P1. The clean local repair is `30e87df`, followed by point-in-time derivation coverage `76d9bb1`, on `codex/v047-b-origin-audit`. Do not begin V0.4.7-C until both commits are integrated, exact-SHA gates pass and B is explicitly closed.

## Closed production baseline

Accepted V0.4.6 production remains exactly:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Final independent V0.4.6 review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md
PASS / COMPLETE
```

V0.4.6 is closed. Do not reopen it without a new compile-valid and behavior-valid contradiction.

## Permanent product contract

PKC is a deterministic Product/System Knowledge Compiler:

```text
source inputs
→ deterministic analyzers/adapters
→ evidence/facts
→ feature/workflow/business-decision candidates
→ knowledge synthesis
→ canonical model
→ portable rendering
```

Do not introduce direct source-to-freeform-AI generation.

Keep these knowledge classes distinct and retained:

```text
business conditions
value lineage / provenance
mutation / causality
```

Conservative authority downgrade must not erase deterministic lower-authority lineage or causal evidence.

## V0.4.7-A — closed

Accepted A checkpoint:

```text
09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
feat: prove bounded reference dynamic lineage
```

A proves:

```text
stored snapshot
ProductGroup.Price → Product.Price → Service.Price

reference / dynamic read
ProductGroup.Price → Service.CurrentGroupPrice
```

It preserves exact project/member/receiver identity, fails closed across known alias/effect/control-flow hazards, retains direct evidence when composition authority is lost, and renders PO-facing lineage separately from business Rules.

Exact A gates passed:

```text
CI + WorkPlay + PokeTrade   35232224202
pinned Loren                35232224014
Loren-main                  35232224029
pinned Jellyfin             35232223958
```

Do not reopen A without a new concrete contradiction.

## V0.4.7-B — review blocker repaired locally, acceptance pending

Delivery note:

```text
docs/reviews/2026-09-17-v0.4.7-b-computation-causality-delivery.md
```

Exact implementation checkpoint:

```text
e5b0d47c82b6db99f5184292730919421a6d2a06
feat: prove V0.4.7-B computation and causality
```

### What B now proves

For the bounded supported straight-line endpoint shape in exact target-project semantic context, PKC can distinguish:

```text
direct copy
vs
multi-input scalar derivation
vs
later override/mutation causality
vs
last proven source before a direct return boundary
```

Positive derivation fixture:

```text
service.Price = 100m
service.Discount = 10m
service.NetPrice = service.Price - service.Discount
return service.NetPrice
```

Runtime result is `90m`. PKC emits a stored `mechanism=derivation` fact with both input occurrences/member identities, exact expression, exact target and source location. Candidate retention, synthesis and workflow Markdown are covered.

Positive override fixture retains the accepted A origin:

```text
group.Price → product.Price → service.Price
```

then observes:

```text
service.Price = 120m
return service.Price
```

PKC retains the original lineage facts, emits a separate `value-causality` fact with `causalRole=override`, references the prior proven value/source, and identifies the override as the last proven source before the supported return.

The generated knowledge intentionally separates:

```text
Value lineage
  derivation
  original origin
  terminal source

State changes
  later override / causality

Rules
  no automatic promotion from B lineage/causality
```

### Fail-closed B boundary

B does not claim authority for unsupported shapes including:

- branch/loop/try/conditional control flow;
- opaque invocations;
- reference-type parameters;
- non-fresh aliases or reference reassignment;
- custom/non-auto modeled scalar accessors;
- unsupported expression operations;
- compound writes such as `+=` to modeled scalar state;
- unary writes such as `++` / `--` after a proven value;
- unresolved target-project semantics.

A static adversarial pass found and fixed the stale-terminal case for compound/unary writes before final acceptance. Permanent regressions require B proof to fail closed there.

No general expression, alias, helper-body, effect or path solver was added.

## B regression coverage

`tests/Pkc.CSharp.Tests/ValueLineageComputationCausalityRegressionTests.cs` covers:

1. executable multi-input derivation `100 - 10 = 90`;
2. scanner fact metadata for all derivation inputs and target;
3. candidate retention → synthesis → workflow Markdown;
4. executable later override returning `120`;
5. retained original A origin after override;
6. separate override causality;
7. exact terminal-source linkage;
8. custom getter fail-closed;
9. invocation-based computation fail-closed;
10. branch computation fail-closed;
11. alias computation fail-closed;
12. compound-write stale-terminal fail-closed;
13. unary-write stale-terminal fail-closed;
14. no project-semantic context => no authoritative B claim.

All accepted V0.4.6 and V0.4.7-A regressions remain green.

## Exact-main verification for B

All required gates passed on exact SHA `e5b0d47c82b6db99f5184292730919421a6d2a06`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35339292475 — PASS
pinned Loren                                35339292546 — PASS
Loren-main canary                           35339292501 — PASS
pinned Jellyfin                             35339292495 — PASS
```

Core:

```text
Release build:       0 warnings / 0 errors
C# tests:            114 / 114 PASS
frontend tests:      13 / 13 PASS
tool pack/install:   PASS
WorkPlay:            PASS
PokeTrade:           PASS
```

Pinned Jellyfin:

```text
commit:                  1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
source build:            0 warnings / 0 errors
facts:                   43,365
relations:               195,316
workflow candidates:     386
product features:        116
canonical Markdown:      504
project-semantic facts:  43,365 / 43,365
portable bundle parity:  PASS
ZIP file-set parity:     PASS
ZIP byte parity:         PASS
raw .pkc leak:           none
src/ leak:               none
artifact id:             10544800985
artifact digest:         sha256:c9884964e48daae5f3b511daf361dd2d0ceb25a06ba6504ca53b041c276106cc
artifact size:           9,162,486 bytes
```

## Independent review result and required continuation

Independent review of `e5b0d47...` is complete. It failed on stale terminal-source authority after unsupported nested and deconstruction writes. The review, executable counterexamples and verification are recorded in `docs/reviews/2026-09-18-v0.4.7-b-independent-review.md`.

Before making C changes:

1. Can a compile-valid supported expression produce a false derivation claim?
2. Can a custom/operator/conversion/accessor effect slip through the supported arithmetic proof?
3. Can a later write make terminal-source evidence stale without invalidation?
4. Does a later override preserve original origin instead of rewriting history?
5. Can same-name or cross-project/member/receiver collisions create a false join?
6. Can `value-causality` or derivation evidence leak into authoritative business Rules?
7. Does Markdown overclaim persistence/general dataflow when only a direct supported return is proven?
8. Is there any compile-valid, runtime-valid contradiction inside the documented straight-line proof boundary?

Integrate `30e87df` and `76d9bb1`, run the repaired exact-SHA remote/cross-benchmark/portable gates, and confirm the nested/deconstruction terminal regressions remain green. Only then mark B PASS / COMPLETE and start C.

## What comes next after a B PASS

V0.4.7-C answers:

> What backend value supplies this response field, and how did it travel through entity/domain state → DTO/projection → API output?

C should be regression-first and bounded. It must prove exact target-project semantic identity and source traceability through the supported backend-to-API path, render the mapping in workflow Markdown, and fail closed on ambiguous projection/alias/conversion/custom accessor/effect shapes.

Do not start D until C passes. Do not start E until D passes. Do not start V0.5 until the V0.4.7/V0.4.x exit gate independently passes.

## Version semantics

Do not conflate:

```text
roadmap milestone
tool/package version
evidence/schema version
```

Current package remains:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Current schema identifiers remain unchanged unless a serialized-contract change is deliberately reviewed and tested.

## Locked state

```text
V0.4.7-A          PASS / COMPLETE
V0.4.7-B          REVIEW BLOCKER FIXED LOCALLY / REMOTE GATES REQUIRED
V0.4.7-C          LOCKED pending repaired B PASS
V0.4.7-D          LOCKED
V0.4.7-E          LOCKED
V0.5 Azure DevOps LOCKED
```
