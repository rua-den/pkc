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
V0.4.7-D API to UI / R7.10 joint visibility      REPAIRED / ALL GATES PASS / PENDING REREVIEW #2
V0.4.7-D overall                                 PENDING INDEPENDENT REREVIEW #2
V0.4.7-E product acceptance                      LOCKED behind D
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.7 acceptance is defined in `docs/v0.4.7-acceptance-plan.md`.
The permanent product contract is `docs/product-knowledge-contract.md`.

## Exact production candidate under review

```text
da5d23771ef8c9d58d0333d1f949e8d742210043
fix: require visible text interpolation
```

This is the exact production code SHA Astra must rereview. A docs-only `[skip ci]` checkpoint may sit on `main` above it. Do not reset `main` to this SHA.

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

V0.4.7-D / R7.9
fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
test: target frontend casing collision

Post-R7.9 mutation-causality blocker repair
67624944da27ff1f1f5a1154018a255aae11d1fe
fix: avoid capturing mutation receiver out parameter
```

## Permanent invariants

Keep these evidence classes distinct:

```text
business conditions
value lineage / provenance
mutation / causality
```

Unsupported inference fails closed. Same/similar names are never sufficient proof. Conservative authority downgrade must retain independently proven lower-authority evidence.

## R7.10 implemented boundary

R7.10 composes only the same exact R7.9-proven value path:

```text
Enumerable.Single/First(predicate)
→ exact selected reference local
→ exact scalar auto-property
→ direct API response property projection
→ explicit wire identity
→ exact R7.9 frontend result/member/state/render identity
→ one authoritative Angular @if around that exact rendered text interpolation
→ joint backend/frontend visibility evidence
```

Frontend visibility cannot upgrade an `observed-only` backend predicate. Zero or multiple supported frontend visibility facts fail closed. Failure to compose R7.10 retains R7.9/C and independently proven evidence.

## Independent rereview #1 — blocker found and repaired

The first independent rereview of the original R7.10 candidate `eb64309263489a2b9bd658762b4526a4a32a8508` found a real render-authority blocker. The R7.9/R7.10 frontend boundary could treat inert or non-visible template text as a rendered UI value.

Compile-valid Angular counterexamples found during rereview:

```html
<ng-template>
  @if (displayPrice > 0) {
    {{ displayPrice }}
  }
</ng-template>

<!-- {{ displayPrice }} -->

<div data-price="{{ displayPrice }}"></div>
```

The repair sequence was regression-first:

```text
05ad6cb937010702a3fd01d5ef756e31bc4e8d9b
fix: fail closed on inert ng-template renders

099fabfcaeecbaf009b75052d20075745ee02437
fix: reject inert commented renders

da5d23771ef8c9d58d0333d1f949e8d742210043
fix: require visible text interpolation
```

Current authority is deliberately conservative: only active visible text interpolation can remain authoritative. Interpolation inside HTML comments, inside an HTML tag/attribute, or under an inert `<ng-template>` ancestor is rejected. This may lose coverage for dynamically instantiated templates; that is accepted fail-closed behavior, not a false-authority regression.

Regression coverage is in:

`tests/Pkc.CSharp.Tests/JointVisibilityTemplateAuthorityRegressionTests.cs`

Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-1.md`

## Exact-SHA verification for repaired candidate

All standard gates passed on exact production SHA `da5d23771ef8c9d58d0333d1f949e8d742210043`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35650084914 — PASS
pinned Loren                                35650084917 — PASS
Loren-main canary                           35650084926 — PASS
pinned Jellyfin + parity/provenance         35650084759 — PASS
```

Core CI evidence:

```text
Release build        0 warnings / 0 errors
C# tests             155 / 155 PASS
frontend tests       13 / 13 PASS
tool pack/install    PASS
WorkPlay build       PASS
PokeTrade            PASS
```

## Repaired pinned three-repository benchmark

The repaired candidate was rerun against the same unchanged pinned repositories.

Because workflow dispatch is not available through the connector, the benchmark used a temporary wrapper branch based exactly on `da5d2377...`:

```text
base implementation: da5d23771ef8c9d58d0333d1f949e8d742210043
branch:              benchmark/r710-rereview1-05ad6cb9
wrapper commit:      d38b21a68e007c1a85a8ec8e906a08f0db440661
run:                 35650761753 — PASS, 3 / 3 jobs
```

GitHub compare confirms the wrapper differs from `da5d2377...` only by one branch-trigger line in `.github/workflows/real-repo-benchmark.yml`; PKC production/test source is byte-identical.

Artifact evidence:

| Repository | Exact target SHA | Target build outcome | PKC exit | Facts | Relations | Knowledge files | Artifact |
| --- | --- | --- | ---: | ---: | ---: | ---: | --- |
| `jin12-xyz/CRM` | `00493af54d4d9e146d1c6eb75f5dc8f3898f09ec` | success | 0 | 415 | 1580 | 25 | `10662460505` |
| `hackersandwizards/agentic-engineering-training-angular` | `22f2aab64617f4de7984370a5bd40e8c9535dbf5` | failure | 0 | 441 | 660 | 26 | `10661878054` |
| `kesetovic/crm-system` | `8e3b74bec4fdcd0144bd65f0c1b49c8e801bd2f7` | failure | 0 | 488 | 2054 | 28 | `10662505428` |

Artifact digests:

```text
jin12       sha256:f846b5384cc6472bf76ee469bfda888d5721664a440d2f18bf2e358d2f130753
agentic     sha256:1a0e1eae1af7cba2c786dfb2c38050387f42dd6f2cc00708af4020ecd9673286
kesetovic   sha256:48e3620862ec46aee4a1dbb119326be81fda60d38f8e44881208b1605bc1d399
```

All three unchanged repositories emit zero current supported cross-layer positives:

```text
R7.9 rendered-value terminal: 0
selected API projection:      0
ui-member-visibility:         0
joint-visibility candidate:   0
combined visibility rule:     0
```

This is expected fail-closed behavior, not R7.14 positive yield. R7.14 remains **NOT YET PASS**.

Agentic mutation-causality cross-check remains closed:

```text
raw UpdatedAt mutation facts:        2
candidate UpdatedAt mutation facts:  0
mutationReceiverOrigin:              runtime-pattern-variable
mutationCausalityBoundary:           caller-object-unproven
transitive-mutation warning:         retained
false PO-facing UpdatedAt claims:    0
```

Detailed benchmark record:

`docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`

## Current external gate

Rereview #1 found blockers and therefore cannot independently certify its own repairs. R7.10 and all of D remain open until a second independent rereview of exact implementation SHA `da5d2377...` passes.

Astra review request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-2-request.md`

If Astra finds no new compile-valid/runtime-valid counterexample, then:

1. mark R7.10 PASS / COMPLETE;
2. mark all V0.4.7-D PASS / COMPLETE;
3. unlock only V0.4.7-E;
4. keep R7.14 required and still NOT PASS;
5. keep V0.5 locked.

Do not start E before that review outcome is recorded.

## Version semantics

```text
roadmap:             V0.4.7-D / R7.10 pending independent rereview #2
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```

No package/schema bump is implied by this checkpoint.
