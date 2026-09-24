# RD8 Runtime/Plugin Real-Repository Evidence Audit

Date: 2026-09-24
Checkpoint: V0.4.7-E0 / RD8 Phase C
Decision: **OPEN — existing external trial artifacts do not exercise runtime/plugin provenance**

This audit answers a narrow RD8 question: can an already-produced public/external real-repository artifact satisfy the RD8 runtime/plugin applicability gate without another operator run?

It does not change RD4 acceptance. RD4 remains deterministically covered by its synthetic provenance regressions. It also does not mark RD8 or E0 PASS.

## Candidate state audited

Current repository HEAD before this audit:

```text
c448173e75a044a87039275bb94d093645a21f88
docs: add reproducible RD8 operator packet [skip ci]
```

The external artifacts inspected were produced by successful GitHub Actions runs for production-equivalent HEAD:

```text
9ef914f57c558e3b2963b9ba4dd187a6ecd6603f
Add question in demo
```

`9ef914f` is docs-only above the latest production code used by the trials. Later `ada5250` and `c448173` are also docs-only, so these artifacts exercise the same production implementation as current main.

Only discovery metadata/counts were inspected. No private source or proprietary payload was involved.

## 1. Jellyfin external trial

Workflow:

```text
.github/workflows/jellyfin-generalization-trial.yml
pinned external repository: jellyfin/jellyfin
pinned source SHA: 1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

Successful PKC run:

```text
GitHub Actions run: 35998770354
artifact: jellyfin-pkc-output
artifact id: 10807945123
head SHA: 9ef914f57c558e3b2963b9ba4dd187a6ecd6603f
```

Discovery artifact inspection:

```text
components: 43
edges: 94
unresolved references: 0
composition probes: 31
```

Edge kinds:

```text
project-reference: 94
runtime-plugin-load: 0
```

No runtime-plugin/runtime-dependency evidence or unresolved runtime identity was present in the profile. The scan plan therefore does not exercise the RD4 runtime-plugin provenance boundary on this pinned Jellyfin source.

Disposition: **NOT APPLICABLE AS RD8-C PROOF**.

## 2. Loren external trial

Workflow:

```text
.github/workflows/loren-external-trial.yml
pinned external repository: rua-den/loren
pinned source SHA: e9e81651d380d7d40998f235cfdc7f119fe67af8
```

Successful PKC run:

```text
GitHub Actions run: 35998770285
artifact: loren-pkc-output
artifact id: 10807093448
head SHA: 9ef914f57c558e3b2963b9ba4dd187a6ecd6603f
```

Discovery artifact inspection:

```text
components: 17
edges: 24
unresolved references: 0
```

Edge kinds:

```text
project-reference: 24
runtime-plugin-load: 0
```

No runtime-plugin/runtime-dependency evidence, plugin-loader identity, or unresolved runtime identity was present. Loren therefore also does not exercise the RD4 runtime-plugin provenance boundary.

Disposition: **NOT APPLICABLE AS RD8-C PROOF**.

## 3. PokeTrade does not qualify as the missing real-repository evidence

The main CI workflow contains `poketrade-real-system`, but its target is:

```text
samples/PokeTradeSystem
```

That is a PKC-owned repository sample/known-answer system, not an independent external real repository. It is useful regression/acceptance evidence for business behavior but must not be promoted into the missing RD8 real-repository runtime/plugin exercise.

Disposition: **SYNTHETIC/OWNED CORPUS — NOT RD8-C REAL-REPOSITORY PROOF**.

## 4. Manual real-repository benchmark corpus has no run artifact to reuse

`real-repo-benchmark.yml` defines a pinned public external corpus and is manual (`workflow_dispatch`). GitHub Actions currently reports zero `workflow_dispatch` runs for this repository, so there is no existing benchmark artifact from that lane to inspect or reuse for RD8-C.

Even if that benchmark is run later, runtime/plugin proof may be claimed only if its discovery profile actually contains the applicable provenance-bearing shape; a successful benchmark run alone is insufficient.

## RD4 boundary remains unchanged

RD4's accepted deterministic rule still requires loader + identity + delivery provenance before creating `runtime-plugin-load`, with ambiguous or unsupported shapes remaining unresolved/UNKNOWN.

The RD4 evidence already records unsupported forms such as folder enumeration plus dynamic DLL load, post-build copy commands, loaders in shared libraries, config-driven plugin lists, and VB sources. This audit does not widen those rules merely to obtain real-repository yield.

## RD8-C decision

Existing external artifacts cannot close RD8 Phase C.

Current evidence matrix:

| Evidence source | External real repo | Runtime/plugin shape exercised | Can close RD8-C? |
| --- | --- | --- | --- |
| Jellyfin trial artifact | yes | no | no |
| Loren trial artifact | yes | no | no |
| PokeTrade sample | no | not qualifying | no |
| manual public benchmark | intended yes | no existing run artifact | no |
| RD4 synthetic regressions | no | yes | no — already proves RD4, not real-repo applicability |

Therefore RD8-C remains an explicit external gate. It may be closed only by one of these two evidence paths:

1. **Accepted N/A for the approved private target** — source-check the target and record that no applicable runtime/plugin topology exists; or
2. **Another approved real repository/corpus** — run current-main discovery and demonstrate the applicable runtime/plugin shape, then verify loader identity provenance, delivery provenance, ownership/test isolation, and fail-closed unresolved behavior.

Do not claim RD8-C PASS merely because RD4 regressions are green or because an unrelated external trial is green.

## Effect on active checkpoint

```text
RD8 Phase A fresh private current-main run       EXTERNAL / REQUIRED
RD8 Phase B targeted Level-1 benchmark           EXTERNAL / REQUIRED
RD8 Phase C runtime/plugin applicability         EXTERNAL / REQUIRED
E0                                                ACTIVE / NOT PASS
E1                                                LOCKED
```

No production repair is justified by this audit: no defect was found. The next action remains the fresh approved private RD8 run and benchmark using `docs/plans/2026-09-24-rd8-current-main-validation-runbook.md`.