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
V0.4.7-D API to UI / R7.10 joint visibility      REPAIRED / ALL GATES PASS / PENDING REREVIEW #12
V0.4.7-D overall                                 PENDING INDEPENDENT REREVIEW #12
V0.4.7-E product acceptance                      LOCKED behind D
R7.14 real-project positive yield                NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`; the permanent product contract is `docs/product-knowledge-contract.md`.

## Exact production candidate under review

```text
f9b20c27820a7ea9ac911222c613c2f9cdfb696f
fix: reject SVG resource renders
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `f9b20c27...`; do not reset `main`.

## Accepted predecessors

```text
V0.4.6     c310e893762997f34562a6b3a62dbab2b05c0c93
V0.4.7-A   09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
V0.4.7-B   17fd30b3a4b8178208adabc12c40dee060bedb54
V0.4.7-C   fbb64b9917da1f63362558355201ff7998384ba0
R7.9       fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
mutation   67624944da27ff1f1f5a1154018a255aae11d1fe
```

Keep these closed unless a new real regression is demonstrated.

## Rereview #11 finding and repair

Independent rereview #11 of predecessor `a3334bc202ee5a9e2dc8cc6d7c176dde8e06c9f9` found another concrete SVG direct-render authority class. Interpolation beneath SVG resource containers such as `<clipPath>`, `<mask>`, `<marker>` and `<pattern>` could be observed by the Angular scanner and promoted as an authoritative directly-rendered member even though those containers define resources rather than directly presenting their child content.

Regression-first repair:

```text
f9b20c27820a7ea9ac911222c613c2f9cdfb696f
fix: reject SVG resource renders
```

`AngularRenderedMemberAuthorityFilter` now treats the bounded set `style`, `defs`, `symbol`, `clipPath`, `mask`, `marker`, and `pattern` as non-direct-render ancestors. A closed resource container before a genuine supported render does not suppress the later positive path.

Regression coverage remains in `tests/Pkc.CSharp.Tests/AngularSvgDefinitionRenderAuthorityRegressionTests.cs`.

Review record: `docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-11.md`.

## Exact-SHA standard verification

All standard gates passed on exact production `f9b20c27820a7ea9ac911222c613c2f9cdfb696f`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35743258917 — PASS
pinned Loren                                35743258977 — PASS
Loren-main canary                           35743259239 — PASS
pinned Jellyfin + parity/provenance         35743258815 — PASS
```

```text
Release build        0 warnings / 0 errors
C# tests             206 / 206 PASS
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
analysis modes        43,365 / 43,365 project-semantic
portable parity       PASS
artifact              10702535277
sha256:fa8a47d770be0d93d78142e116b89858750cfb04d0d884f77b2b3ce00bc710bf
```

## Final pinned three-repository safety benchmark

```text
base production: f9b20c27820a7ea9ac911222c613c2f9cdfb696f
wrapper commit:  461a281bd9c6d6e3a9d5b9b95c68562ff41638a9
run:             35743389433 — PASS, 3 / 3 jobs
```

The wrapper differs from production by exactly one workflow branch-trigger line. Direct artifact inspection for every pinned repository found:

```text
R7.9 rendered-value terminal: 0
selected API projection:      0
ui-member-visibility:         0
joint-visibility candidate:   0
combined visibility rule:     0
```

Interpretation:

```text
Safety / fail-closed: PASS
R7.14 positive real-project yield: NOT PASS
```

Agentic mutation-causality remains closed: exactly two raw `UpdatedAt` mutations retain `runtime-pattern-variable / caller-object-unproven`, with zero feature-candidate, product-feature or Markdown promotion.

## Current external gate

Required next gate:

```text
independent rereview #12 of exact f9b20c27820a7ea9ac911222c613c2f9cdfb696f
```

Request: `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-12-request.md`.

If rereview #12 finds no new compile-valid/runtime-valid false-positive blocker, it may mark R7.10 and all of D PASS / COMPLETE and unlock only E. R7.14 remains required for E; V0.5 remains locked until E completes.

## Version semantics

```text
roadmap:             V0.4.7-D / R7.10 pending independent rereview #12
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```
