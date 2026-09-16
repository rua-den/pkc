# PKC Handoff

Use this file when continuing PKC in another coding or review thread.

## Read first

1. `docs/status.md`
2. this handoff
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/v0.4.7-acceptance-plan.md`
6. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md`
7. `docs/reviews/2026-09-16-v0.4.7-plan-readiness-review.md` for the rationale already incorporated into the acceptance plan

## Repository checkpoint

V0.4.7 acceptance planning was introduced in `024e36c`; the readiness review was committed in `edf59ed`. Both are documentation checkpoints, not lineage implementation. This reconciliation incorporates that review into the active plan.

Before coding, inspect `git status`, HEAD, and recent commits; verify the accepted production below is an ancestor and inspect any implementation changes since it. Start from the current agreed checkout, not an obsolete planning SHA. Do not assume HEAD must equal a particular documentation commit, and do not overwrite newer work.

Accepted V0.4.6 production remains exactly:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Do not reset to, branch from, amend, or modify the accepted production SHA as part of V0.4.7 planning/history cleanup.

## Current milestone state

```text
V0.4.4  Loren knowledge readiness              PASS / COMPLETE
V0.4.5  Jellyfin generalization               PASS / COMPLETE
V0.4.6  business logic reconstruction         PASS / COMPLETE
V0.4.7  cross-layer PO-question readiness     CURRENT / NEXT MILESTONE
V0.5    Azure DevOps input evidence           LOCKED
```

V0.4.6 is closed by `docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md`.

Do not reopen B6.1-B6.4 without a new compile-valid and behavior-valid contradiction.

## Permanent product contract

PKC is a deterministic Product/System Knowledge Compiler. The portable knowledge pack must allow a Product Owner to ask practical system/product questions without requiring the AI to re-read source code.

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

Permanent guardrail: `docs/product-knowledge-contract.md`.

Keep distinct and retain:

```text
business conditions
value lineage / provenance
mutation / causality
```

These may be connected in explanations but must not be silently promoted into one another.

Conservative product-rule downgrade must not erase deterministic lower-authority lineage, mutation or causal evidence.

## V0.4.6 closed scope

```text
B6.1 PASS — exact C# invocation semantic identity
B6.2 PASS — conservative configured-item ownership
B6.3 PASS — module-qualified Angular service ownership
B6.4 PASS — observable predicate authority is conservative across projection, callback, constructor and Queryable-provider boundaries
```

Accepted B6.4 boundaries include:

1. local/discarded predicates downgrade;
2. transformed/polarity-changing return contexts fail closed unless modeled;
3. arbitrary `Select` is not preserving by default;
4. direct identity projection uses symbol identity;
5. unsupported predicate dependencies fail closed;
6. defensive clone members require safe direct same-member copies;
7. custom setter/nested initializer/output effects fail closed;
8. callback/comparer-bearing ordering/equality paths do not preserve authority by target name alone;
9. callback-free Enumerable preservation is an audited exact-shape subset;
10. same-type clone construction must be proven inert;
11. exact Queryable predicates are observed-only without provider-semantics proof;
12. an Enumerable `Where` path loses authority after any Queryable hop;
13. downgraded predicate evidence remains through `observes-predicate`.

## V0.4.7 acceptance contract

Concrete scope and checkpoint exit gates are maintained in:

```text
docs/v0.4.7-acceptance-plan.md
```

The required PO questions include:

```text
Where did this value originally come from?
If the upstream value changes later, does the existing downstream value change automatically?
What code path can change this value after creation?
Was this value directly copied or computed?
What was the last observed source before the value was persisted or returned?
What backend conditions and frontend conditions jointly determine whether an item is visible?
How did the value move through backend → DTO/projection → API → frontend composition?
If authority is incomplete, what lineage or causal evidence is still deterministically known?
```

The canonical backend lineage fixture begins with:

```text
ProductGroup.Price
→ Product.Price
→ Service.Price
```

Each edge must be classified from proof as applicable:

```text
copy
snapshot
derivation
reference/dynamic
override
mutation
```

The blocking identity/collision negative includes:

```text
Product.Price
Service.Price
Dto.Price
Component.price
```

PKC must never connect these merely because names match.

## V0.4.7 regression gate sequence

The acceptance plan defines R7.1-R7.13. The intended implementation order is:

```text
V0.4.7-A
backend origin/copy timing in workflow knowledge
+ exact assignment, receiver/path and storage proof
+ collision and false-composition negatives
+ snapshot vs dynamic positives

V0.4.7-B
derivation
+ last source before persist/return
+ later mutation/override causality

V0.4.7-C
DTO/projection
+ API output lineage

V0.4.7-D
frontend result binding/composition
+ frontend visibility/filter conditions
+ joint backend/frontend explanation

V0.4.7-E
final knowledge-only acceptance
+ full portable parity/no-leak
+ cross-benchmark exact-SHA gates
```

Do not jump ahead to later slices while an earlier identity/semantics gate is red.

Every checkpoint renders its new answers; E is not the first rendering step. The acceptance plan owns the full gate definitions rather than this summary.

## Version semantics — do not conflate

There are three independent version domains:

```text
roadmap milestone version
tool/package version
evidence/schema version
```

Current examples:

```text
roadmap milestone:        V0.4.7
tool/package:             RuaDen.Pkc.Tool 0.4.3-preview.2
raw C# evidence schema:   0.4.4-csharp-raw
merged facts schema:      0.4.4
cross-stack candidates:   0.4.6
frontend evidence schema: 0.4.3-frontend
```

Roadmap progress does not mechanically bump package or schema versions.

Schema strings change only when their serialized contract/semantics change and that change is regression-documented.

Do not bump the NuGet/.NET tool package in the first V0.4.7 implementation checkpoint unless a separate release decision explicitly requires it.

## V0.4.6 warnings carried forward

These are non-blocking V0.4.7 considerations, not reopened V0.4.6 blockers:

```text
W10.1
Observed-only predicate Evidence is separated from Rules but does not explicitly render the `observed-only` label.

W10.2
Queryable names remain in old safe-operation sets but are unreachable behind the Queryable fail-closed guard.
```

Only address them when the touched V0.4.7 scope makes doing so coherent and regression-safe.

## Documentation ownership and current delivery

Current-state documents must agree that:

```text
V0.4.6 COMPLETE
V0.4.7 CURRENT
V0.5 LOCKED
```

`docs/vision.md` defines product success; `docs/status.md` records implementation/evidence state; `docs/milestones.md` defines delivery order; `docs/v0.4.7-acceptance-plan.md` owns detailed checkpoint gates. Keep README and this handoff as consistent summaries. Preserve historical reviews and benchmark evidence rather than relabeling them as current proof.

Current step is the first copy/snapshot slice within A, not completed A. Every checkpoint must reach generated PO-readable knowledge; rendering starts in A. E is the final knowledge-only, parity/no-leak and generalization acceptance. Do not use a growing raw fact count or all-unknown answers as evidence of success.

## Accepted V0.4.6 gate evidence

Exact production `c310e893762997f34562a6b3a62dbab2b05c0c93`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   34990080620 — PASS
pinned Loren                                34990080707 — PASS
Loren-main canary                           34990080551 — PASS
pinned Jellyfin                             34990080546 — PASS
```

Pinned Jellyfin portable parity/no-leak remained PASS with artifact:

```text
10405810551
sha256:8664945310d5fd0da3a0c838b001cc5fa343174410dc1ac6d05b1335e41b0257
9,159,880 bytes
```

These gates close V0.4.6; they do not replace new V0.4.7 regression proof.

## Exact next implementation action

Start **V0.4.7-A**, regression-first.

Add a compile-valid, executable, endpoint-backed C# fixture proving:

```text
ProductGroup.Price → Product.Price → Service.Price
```

as exact project-backed direct-copy snapshot edges that survive scanner → candidate → synthesis → workflow Markdown. The generated workflow must explain both edges, their source locations, and why an upstream scalar change alone leaves the stored copies unchanged.

In the same regression checkpoint add unrelated same-name members:

```text
Product.Price
Service.Price
Dto.Price
Component.price
```

and assert that no lineage edge is created without explicit deterministic dataflow. Apply the acceptance plan's A proof boundary: distinct receivers, incompatible branches, intervening writes, setter/conversion effects, shared-reference content, and unresolved semantic context must not create false composed/copy/snapshot claims. Existing mutation metadata and display names are not fresh assignment proof.

First prove meaningful semantic/output RED against current behavior. Then implement only the proof, retention, synthesis and workflow rendering needed for the supported slice. Keep unsupported cases as qualified observations/unknowns. A's separate dynamic-read positive remains required; do not mark A complete from the first snapshot slice alone.

Run focused tests locally, then the relevant full tests and Release build sequentially. Review the complete diff and the generated answer. Save a coherent commit, publish through an authorized remote action, and verify the existing exact-SHA checkpoint gates. CI is final verification, not the edit loop. Record any pending remote gate honestly; a documentation review is not implementation acceptance.

Do **not** begin B/C/D until A's identity, collision, snapshot/dynamic and portable delivery gates are green. Do not introduce unrelated refactors, new inputs, or a general alias/path solver to expand the supported slice.

Do not start Azure DevOps ingestion:

```text
V0.5 LOCKED until the V0.4.7 / V0.4.x PO-question-readiness exit gate passes
```
