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
V0.4.7-D mutation-causality benchmark blocker    PASS / CLOSED
V0.4.7-D API to UI / R7.10 joint visibility      REPAIRED / ALL GATES PASS / PENDING REREVIEW #3
V0.4.7-D overall                                 PENDING INDEPENDENT REREVIEW #3
V0.4.7-E product acceptance                      LOCKED behind D
R7.14 real-project positive yield                NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`.
The permanent product contract is `docs/product-knowledge-contract.md`.

## Exact production candidate under review

```text
97161baa2d0aff9131a7acf9db752393ae913d64
fix: reject statically hidden rendered text
```

This is the exact production SHA for the next independent rereview. A docs-only `[skip ci]` checkpoint may sit above it on `main`; do not reset `main` to this SHA.

## Closed production baselines

```text
V0.4.6
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority

V0.4.7-A
09a0c7f2a058adfd7ddb7b4ac2feb2d17580a429
feat: prove bounded reference dynamic lineage

V0.4.7-B
17fd30b3a4b8178208adabc12c40dee060bedb54
fix: fail closed after opaque terminal effects

V0.4.7-C
fbb64b9917da1f63362558355201ff7998384ba0
feat: prove backend API projection lineage

V0.4.7-D / R7.9 baseline
fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
test: target frontend casing collision

Post-R7.9 mutation-causality repair
67624944da27ff1f1f5a1154018a255aae11d1fe
fix: avoid capturing mutation receiver out parameter
```

Keep accepted predecessors closed unless a real regression is demonstrated.

## Permanent invariants

Keep these evidence classes distinct:

```text
business conditions
value lineage / provenance
mutation / causality
```

Unsupported inference fails closed. Same/similar names are never sufficient proof. Conservative authority downgrade must retain independently proven lower-authority evidence.

## R7.10 intended bounded proof

For the same exact R7.9-proven value path:

```text
Enumerable.Single/First(predicate)
→ exact selected reference local
→ exact scalar auto-property
→ direct API response property projection
→ explicit wire identity
→ exact frontend result/member/state identity
→ authoritative active visible Angular text interpolation
→ one supported enclosing @if
→ joint backend/frontend visibility evidence
```

Frontend visibility cannot upgrade an `observed-only` backend predicate. Zero, multiple, nested or otherwise unsupported visibility paths fail closed. Failure to compose R7.10 retains independently proven C/R7.9/lower-authority evidence.

## Rereview history

Independent rereview #1 found false rendered-value authority for:

```text
interpolation under inert <ng-template>
interpolation inside HTML comments
interpolation inside HTML tags/attributes
```

Those were repaired in:

```text
05ad6cb937010702a3fd01d5ef756e31bc4e8d9b
099fabfcaeecbaf009b75052d20075745ee02437
da5d23771ef8c9d58d0333d1f949e8d742210043
```

Independent rereview #2 then found a distinct runtime-valid false-authority shape:

```html
<section hidden>
  @if (displayPrice > 0) {
    <strong>{{ displayPrice }}</strong>
  }
</section>
```

Because the standard HTML boolean `hidden` attribute suppresses presentation of the subtree, the exact interpolation must not be promoted to a user-visible rendered-value terminal. Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-2.md`

## Static-hidden repair

Production repair:

```text
97161baa2d0aff9131a7acf9db752393ae913d64
fix: reject statically hidden rendered text
```

The render-authority filter now tracks bounded HTML ancestry and rejects simple interpolation beneath an ancestor with a static `hidden` boolean attribute. Presence semantics are honored, so `hidden="false"` is still statically hidden. Closed hidden siblings are popped correctly. Dynamic Angular bindings such as `[hidden]="false"` are not interpreted as static hidden and remain outside this repair's claimed dynamic semantics. Ambiguous ancestor structure fails closed.

Regression coverage is in:

```text
tests/Pkc.CSharp.Tests/JointVisibilityTemplateAuthorityRegressionTests.cs
tests/Pkc.CSharp.Tests/JointVisibilityRegressionTests.cs
```

The end-to-end regression requires the hidden-ancestor shape to produce:

```text
no authoritative ui-member-render
no ui-member-visibility
no R7.9 rendered UI value terminal
no R7.10 joint-visibility fact
```

Local .NET execution was unavailable in the implementation environment, so no claim of local test execution is made. The complete source/test diff was reviewed before the single production push, and clean exact-SHA CI below provides executable verification.

## Exact-SHA standard verification

All standard gates passed on exact production SHA `97161baa2d0aff9131a7acf9db752393ae913d64`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35686749488 — PASS
pinned Loren                                35686749493 — PASS
Loren-main canary                           35686749523 — PASS
pinned Jellyfin + parity/provenance         35686749575 — PASS
```

Core CI evidence:

```text
Release build        0 warnings / 0 errors
C# tests             161 / 161 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
WorkPlay             PASS
PokeTrade            PASS
```

Pinned Jellyfin evidence:

```text
source build          PASS, 0 warnings / 0 errors
facts                 43,365
relations             195,316
workflow candidates   386
product features      116
knowledge Markdown    504 files
analysis modes         43,365 / 43,365 project-semantic
portable parity       PASS
artifact              10676304879
sha256:7dcfed74eadb0146ab422c497953d8054ed01d5fddb6b6e6013a37b7729fd28b
```

## Repaired pinned three-repository benchmark

Temporary wrapper branch based exactly on production SHA `97161baa...`:

```text
branch:         benchmark/r710-hidden-97161baa
wrapper commit: 850eea0b64f464bdc1a5ea536adb39d4b912c650
run:            35687153720 — PASS, 3 / 3 jobs
```

GitHub compare confirms the wrapper differs from `97161baa...` only by the benchmark workflow trigger branch line; production and test source are identical.

Artifact inspection found the same conservative result on all three unchanged pinned repositories:

```text
R7.9 rendered-value terminal: 0
selected API projection:      0
ui-member-visibility:         0
joint-visibility candidate:   0
combined visibility rule:     0
```

Agentic mutation-causality remains closed:

```text
raw UpdatedAt mutation facts:        2
candidate UpdatedAt mutation facts:  0
mutationReceiverOrigin:              runtime-pattern-variable
mutationCausalityBoundary:           caller-object-unproven
transitive-mutation warning:         retained
false PO-facing UpdatedAt claims:    0
```

Detailed evidence:

`docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`

This benchmark is fail-closed stability evidence only. It does **not** satisfy R7.14; all three repositories still emit zero supported positive R7.9/R7.10 cross-layer answer.

## Current external gate

This implementation session repaired the blocker and therefore must not self-certify its own repair. Required next gate:

```text
independent rereview #3 of exact 97161baa2d0aff9131a7acf9db752393ae913d64
```

Request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-3-request.md`

If rereview #3 finds no new compile-valid/runtime-valid counterexample, it may mark R7.10 and all of D PASS / COMPLETE and unlock only E. R7.14 remains required and NOT PASS; V0.5 remains locked.

Do not start E before that independent outcome is recorded.

## Version semantics

```text
roadmap:             V0.4.7-D / R7.10 pending independent rereview #3
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```

No package/schema bump is implied by this checkpoint.
