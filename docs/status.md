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
V0.4.7-D API to UI / R7.10 joint visibility      CURRENT / UNLOCKED
V0.4.7-E product acceptance                      LOCKED behind D
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`.
The permanent product contract is `docs/product-knowledge-contract.md`.

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
```

Final C rereview: `docs/reviews/2026-09-19-v0.4.7-c-final-rereview.md`.

## Permanent product-knowledge invariants

Keep these knowledge classes distinct and retain deterministic lower-authority evidence when stronger authority fails:

```text
business conditions
value lineage / provenance
mutation / causality
```

Unsupported inference must fail closed rather than produce a false PO-facing claim.

## V0.4.7-C — PASS / COMPLETE

C proves, inside its bounded target-project-semantic shape:

```text
backend entity/domain scalar auto-property
→ explicit DTO/response scalar auto-property assignment
→ direct API response object initializer
```

The accepted positive is equivalent to:

```text
ProductEntity.Price
→ PriceResponse.DisplayPrice
→ API response
```

C uses exact project/assembly/type/member semantics and retains source member, target member, projection and response locations. It emits separate `value-transfer` `api-projection` evidence and `value-terminal-source` `API response field` evidence. Failure to prove a stronger later layer must not remove C.

## V0.4.7-D / R7.9 — PASS / COMPLETE

Exact implementation/test checkpoint:

```text
fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
test: target frontend casing collision
```

R7.9 now proves the first supported property-level API → UI lineage only through explicit wire identity and exact bounded frontend dataflow:

```text
C-proven backend response property
→ [JsonPropertyName("displayPrice")]
→ exact API response field
→ typed frontend HTTP result contract member `displayPrice`
→ exact resolved service + API method
→ exact subscribe result receiver/member
→ component/view-model assignment
→ exact authoritative Angular interpolation
→ rendered UI value
```

Route identity is used only to scope endpoint ↔ frontend API-call correlation. It is not sufficient for the property-level edge. No normalized-name, casing, textual-similarity or camelCase/PascalCase convention guessing is accepted.

### R7.9 deterministic evidence retained

The composed rendered-value lineage retains deterministic evidence for:

- backend C projection and API response boundary;
- explicit `JsonPropertyName` declaration and source location;
- frontend typed API call;
- frontend result-contract member and declaration location;
- exact subscribe result-member assignment;
- component/view-model member identity;
- authoritative render location;
- endpoint, service, method, response type and wire identity.

The proven chain reaches candidate → knowledge synthesis → PO-facing Markdown.

### R7.9 permanent fail-closed regressions

Coverage proves that R7.9 is not created for:

- same/equivalent `DisplayPrice` / `displayPrice` casing/name collisions without explicit wire proof;
- an unrelated resolved service/result that happens to expose the same member name;
- unresolved or ambiguous service receivers;
- wrong/unresolved subscribe result receivers / RHS expressions;
- untyped or fallback-only frontend HTTP evidence;
- missing `JsonPropertyName` replaced by naming-convention guesses;
- non-authoritative rendered-member evidence.

When D composition fails, accepted C evidence remains intact, including `api-projection`, `API response field`, and existing C value lineage.

PokeTrade was not modified to manufacture the R7.9 fixture.

### Exact-SHA verification for R7.9

All required exact-SHA gates passed on `fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35588410936 — PASS
pinned Loren                                35588410941 — PASS
Loren-main canary                           35588410930 — PASS
pinned Jellyfin                             35588410939 — PASS
```

Core CI:

```text
Release build:       0 warnings / 0 errors
C# tests:            142 / 142 PASS
frontend tests:      13 / 13 PASS
tool pack/install:   PASS
WorkPlay:            PASS
PokeTrade:           PASS
```

Pinned Jellyfin completed source build, PKC knowledge compilation, portable handoff parity verification and artifact upload successfully.

Local `.NET` execution was not available in the current agent environment, so these are clean-environment exact-SHA CI results; no unsupported local-test claim is recorded.

## V0.4.7-D mutation-causality benchmark blocker — PASS / CLOSED

The post-R7.9 real-repository benchmark exposed a false transitive mutation claim across `SaveChangesAsync() → SetTimestamps()` when the helper selected runtime entities through patterns such as `entry.Entity is User user`. PKC incorrectly promoted both `user.UpdatedAt` and `contact.UpdatedAt` into unrelated endpoint workflows.

The generic repair preserves the raw mutation facts, marks runtime pattern-selected receivers with an unproven caller-object causality boundary, and fails closed only at workflow promotion. Ordinary local/domain-object transitive mutations and self-owned domain mutations remain eligible.

Exact accepted implementation checkpoint:

```text
67624944da27ff1f1f5a1154018a255aae11d1fe
fix: avoid capturing mutation receiver out parameter
```

Exact-SHA standard gates:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35632901150 — PASS
pinned Loren                                35632901064 — PASS
Loren-main canary                           35632901146 — PASS
pinned Jellyfin + parity/provenance         35632901144 — PASS
```

Core verification:

```text
Release build:       0 warnings / 0 errors
C# tests:            145 / 145 PASS
frontend tests:      13 / 13 PASS
tool pack/install:   PASS
WorkPlay:            PASS
PokeTrade:           PASS, including product-knowledge verification
```

Pinned three-repository benchmark rerun:

```text
base implementation: 67624944da27ff1f1f5a1154018a255aae11d1fe
benchmark wrapper:   ea727bea808d79526f786d6092810ec368338c20
run:                 35633768883 — PASS, 3 / 3 matrix jobs
```

The temporary wrapper differs from the accepted main checkpoint by one workflow-trigger line only. All PKC source/test logic is byte-identical to `67624944...`.

Benchmark artifacts prove:

- PKC exits `0` on all three unchanged repositories;
- the original agentic-angular raw `user.UpdatedAt` and `contact.UpdatedAt` facts remain present;
- both facts are labeled `runtime-pattern-variable` with `caller-object-unproven` causality;
- those two AppDbContext timestamp mutations appear in **0** feature candidates;
- PO-facing knowledge contains **0** false `UpdatedAt` timestamp claims;
- affected workflows carry a transitive-mutation uncertainty warning instead;
- legitimate PokeTrade transitive domain mutations remain intact.

Real-repository R7.9 rendered-value positive yield remains `0 / 3`; R7.14 is therefore still **NOT YET PASS** and must not be claimed complete.

Full details: `docs/benchmarks/2026-09-21-real-repo-r7.9-benchmark.md`.

## V0.4.7-D / R7.10 — CURRENT / UNLOCKED

D is not complete yet. The next demo-critical checkpoint is joint backend/frontend visibility for the **same R7.9-proven item/dataflow path**:

```text
proven API → UI item/value identity
+
backend selection / eligibility condition evidence
+
frontend visibility / filtering condition evidence
→ joint PO-facing explanation with authorities kept separate
```

Frontend visibility evidence must never upgrade an observed-only or lower-authority backend condition into an authoritative business rule. Unrelated predicates must remain disconnected.

Do not start E until D/R7.10 passes its focused/full/cross-benchmark gates and required review.

## Version semantics

Roadmap, package and serialized schema versions remain independent:

```text
roadmap:             V0.4.7-D / R7.10 current
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```

No package/schema bump is implied by R7.9 completion or the mutation-causality blocker repair.

## Carried non-blocking warnings

```text
W10.1
Observed-only predicate Evidence is separated from Rules but does not explicitly render the `observed-only` label.

W10.2
Queryable names remain in old safe-operation sets but are unreachable behind the Queryable fail-closed guard.
```

## Exact next action

Continue **V0.4.7-D only** with R7.10 regression-first. Reuse the exact R7.9 item/dataflow identity as the composition key; prove joint backend/frontend visibility without weakening either authority boundary.

Do not broaden the pre-demo scope merely to manufacture R7.14 real-project positive yield.

```text
V0.4.7-E LOCKED until D passes
V0.5 LOCKED until the V0.4.7 / V0.4.x PO-question-readiness exit gate passes
```
