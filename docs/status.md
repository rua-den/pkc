# PKC Status

Last updated: 2026-09-22

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7-A origin and copy timing                  PASS / COMPLETE
V0.4.7-B computation and later change            PASS / COMPLETE
V0.4.7-C backend to API                          PASS / COMPLETE
V0.4.7-D API to UI / R7.9 binding                PASS / COMPLETE
V0.4.7-D mutation-causality blocker              PASS / CLOSED
V0.4.7-D API to UI / R7.10 joint visibility      REPAIRED / ALL GATES PASS / PENDING REREVIEW #9
V0.4.7-D overall                                 PENDING INDEPENDENT REREVIEW #9
V0.4.7-E product acceptance                      LOCKED behind D
R7.14 real-project positive yield                NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`; the permanent product contract is `docs/product-knowledge-contract.md`.

## Exact production candidate under review

```text
cd3b1d4b64168c4e95284299c5715b3db0e4da4b
fix: fail closed on structural visibility directives
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `cd3b1d4b...`; do not reset `main`.

## Accepted predecessors

```text
V0.4.6     c310e893762997f34562a6b3a62dbab2b05c0c93
V0.4.7-A   09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
V0.4.7-B   17fd30b3a4b8178208adabc12c40dee060bedb54
V0.4.7-C   fbb64b9917da1f63362558355201ff7998384ba0
R7.9       fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
mutation   67624944da27ff1f1f5a1154018a255aae11d1fe
```

Keep these closed unless a real regression is demonstrated.

## Permanent invariants

Keep business conditions, value lineage/provenance, mutation/causality, render authority and visibility authority distinct. Unsupported inference fails closed. Same/similar names are never sufficient proof. Stronger composition failure must preserve independently proven lower-authority evidence.

## R7.10 bounded proof

For the same exact R7.9-proven value path:

```text
Enumerable.Single/First(predicate)
→ exact selected reference local
→ exact scalar auto-property
→ direct API response property projection
→ explicit wire identity
→ exact frontend result/member/state identity
→ authoritative active rendered Angular text interpolation
→ exactly one supported enclosing @if
→ no additional unsupported control/structural visibility authority
→ joint backend/frontend visibility evidence
```

Frontend visibility cannot upgrade an `observed-only` backend predicate. Zero, multiple, nested or unsupported visibility paths fail closed.

## Rereview #8 and hardening train

Independent rereview #8 found two distinct compile-valid/runtime-valid authority bypasses on predecessor production:

1. `ngNonBindable` can make `{{ displayPrice }}` literal text, while the old render filter could still authorize the member value.
2. A `>` inside a quoted HTML attribute could fool the old tag-boundary heuristic and promote attribute interpolation as visible text.

The implementation session then adversarially hardened the same boundary before handoff:

```text
a36fae917aa687d8b0240514b6144271e6d81b89  fix: harden Angular render authority
e9e22cefc969a058114eee0cc443270d6f5d0900  fix: reject quoted attribute visibility controls
3299e54a1bafdedd5146a86efd58df8e5c8dae99  fix: bound Angular visibility control flow
cd3b1d4b64168c4e95284299c5715b3db0e4da4b  fix: fail closed on structural visibility directives
```

The final candidate additionally fails closed when a render is controlled by unsupported nested Angular blocks (`@for`, `@defer`, etc.) or any `*structuralDirective` ancestor such as `*ngIf` / `*ngFor`, while preserving the independently proven R7.9 render fact.

Review record: `docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-8.md`.

## Exact-SHA standard verification

All standard gates passed on exact production `cd3b1d4b64168c4e95284299c5715b3db0e4da4b`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35714413695 — PASS
pinned Loren                                35714413680 — PASS
Loren-main canary                           35714413696 — PASS
pinned Jellyfin + parity/provenance         35714413688 — PASS
```

```text
Release build        0 warnings / 0 errors
C# tests             194 / 194 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
WorkPlay             PASS
PokeTrade            PASS
```

Pinned Jellyfin:

```text
source build          PASS, 0 warnings / 0 errors
facts                 43,365
relations             195,316
workflow candidates   386
product features      116
knowledge Markdown    504 files
analysis modes         43,365 / 43,365 project-semantic
portable parity       PASS
artifact              10688588126
sha256:140afc4cf9be4aa1c0961c4372a34d3c1d73abfbfabe805106adb839dab6c106
```

## Final pinned three-repository safety benchmark

```text
base production: cd3b1d4b64168c4e95284299c5715b3db0e4da4b
wrapper commit:  41c73a4c6a16065c49981bbd59b3e6dbc022b3cc
run:             35714494308 — PASS, 3 / 3 jobs
```

Direct artifact inspection for every pinned repository:

```text
R7.9 rendered-value terminal: 0
selected API projection:      0
ui-member-visibility:         0
joint-visibility candidate:   0
combined visibility rule:     0
```

Interpretation:

```text
Safety question: can PKC run on unchanged real repositories without manufacturing unsupported cross-layer authority?
Answer: YES — PASS 3/3.

Positive-yield question: does at least one unchanged real repository naturally contain the exact supported full cross-layer shape?
Answer: NOT YET — this is R7.14 and remains NOT PASS.
```

Agentic mutation-causality remains closed: exactly two raw `UpdatedAt` mutations, both `runtime-pattern-variable / caller-object-unproven`, with zero candidate promotion.

## Current external gate

Required next gate:

```text
independent rereview #9 of exact cd3b1d4b64168c4e95284299c5715b3db0e4da4b
```

Request: `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-9-request.md`.

If rereview #9 finds no new compile-valid/runtime-valid false-positive blocker, it may mark R7.10 and all of D PASS / COMPLETE and unlock only E. R7.14 remains required for E; V0.5 remains locked until E completes.

## Version semantics

```text
roadmap:             V0.4.7-D / R7.10 pending independent rereview #9
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```
