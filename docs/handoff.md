# PKC Handoff

Last updated: 2026-09-22

Use this file when continuing PKC in another coding/review thread.

## Read first

Before changing production code, read:

1. `docs/status.md`
2. this handoff
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/v0.4.7-acceptance-plan.md`
6. independent R7.10 rereview records #1 through #8
7. `docs/benchmarks/2026-09-22-r7.10-real-repo-benchmark.md`
8. `docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-9-request.md`

Then inspect current `main`, recent commits and repository status. Never reset to a historical SHA merely because this handoff names it.

## Current production checkpoint

Exact production SHA for fresh independent rereview:

```text
cd3b1d4b64168c4e95284299c5715b3db0e4da4b
fix: fail closed on structural visibility directives
```

A docs-only `[skip ci]` checkpoint may sit above this SHA on `main`. Review production behavior at `cd3b1d4b...`; do not reset `main`.

## Current milestone state

```text
V0.4.7-A                                PASS / COMPLETE
V0.4.7-B                                PASS / COMPLETE
V0.4.7-C                                PASS / COMPLETE
V0.4.7-D / R7.9                         PASS / COMPLETE
V0.4.7-D mutation-causality blocker     PASS / CLOSED
V0.4.7-D / R7.10                        REPAIRED / ALL GATES PASS / PENDING REREVIEW #9
V0.4.7-D overall                        PENDING INDEPENDENT REREVIEW #9
V0.4.7-E                                LOCKED behind D
R7.14 real-project positive yield       NOT PASS / REQUIRED FOR E
V0.5 Azure DevOps                       LOCKED
```

Do not start E or V0.5 before the independent D outcome.

## Permanent contract

Keep business conditions, value lineage/provenance, mutation/causality, render authority and visibility authority distinct. Unsupported inference fails closed. Same/similar names are not proof. Stronger composition failure must preserve independently proven lower-authority evidence.

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

## R7.10 bounded positive

R7.10 answers, for the same exact R7.9-proven value path:

> What backend selection condition and frontend visibility condition jointly determine whether this rendered value is visible?

Supported backend remains target-project-semantic exact `System.Linq.Enumerable.Single/First(predicate)` → exact selected local → direct response property projection. R7.9 supplies explicit wire identity, typed result member, exact assignment and authoritative active rendered text interpolation. R7.10 accepts exactly one supported enclosing Angular `@if` and composes only exact fact IDs.

Frontend evidence cannot upgrade an `observed-only` backend condition. Any extra unsupported visibility authority fails closed while retaining R7.9 evidence.

## Rereview #8 outcome

Independent rereview #8 challenged predecessor production and found distinct render-authority false positives:

```html
@if (isAllowed) {
  <strong ngNonBindable>{{ displayPrice }}</strong>
}
```

`ngNonBindable` makes the interpolation literal text, so it cannot prove rendered member-value authority.

It also found quoted-attribute boundary ambiguity such as:

```html
@if (isAllowed) {
  <div title="price > {{ displayPrice }}"></div>
}
```

The old `<` / `>` heuristic could treat the interpolation as text after the `>` inside the quoted value.

Review record:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-independent-rereview-8.md`

## Repair and additional hardening

The implementation session repaired the review blockers and then challenged the same boundary before handoff:

```text
a36fae917aa687d8b0240514b6144271e6d81b89  fix: harden Angular render authority
e9e22cefc969a058114eee0cc443270d6f5d0900  fix: reject quoted attribute visibility controls
3299e54a1bafdedd5146a86efd58df8e5c8dae99  fix: bound Angular visibility control flow
cd3b1d4b64168c4e95284299c5715b3db0e4da4b  fix: fail closed on structural visibility directives
```

Final behavior:

- `ngNonBindable` subtree cannot create R7.9 rendered-member authority.
- HTML tag/attribute boundaries are quote-aware.
- fake `@if` inside HTML comments/tags/quoted attributes cannot create R7.10 authority.
- brace matching ignores HTML tags/interpolation/comment content rather than treating plain text quotes as code strings.
- render under nested/extra Angular blocks (`@for`, `@defer`, etc.) does not create R7.10 visibility authority.
- render under any `*structuralDirective` ancestor fails closed for R7.10 while preserving the R7.9 render fact.
- the supported exact single-`@if` positive remains covered.

## Exact-SHA verification

Exact production `cd3b1d4b64168c4e95284299c5715b3db0e4da4b`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35714413695 — PASS
pinned Loren                                35714413680 — PASS
Loren-main canary                           35714413696 — PASS
pinned Jellyfin + parity/provenance         35714413688 — PASS
```

Core CI:

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
43,365 facts | 195,316 relations | 386 workflow candidates
116 product features | 504 knowledge Markdown files
43,365 / 43,365 facts project-semantic
portable parity/no-leak PASS
artifact 10688588126
sha256:140afc4cf9be4aa1c0961c4372a34d3c1d73abfbfabe805106adb839dab6c106
```

## Real-repository benchmark — how to read it

Final wrapper based on exact `cd3b1d4b...`:

```text
wrapper commit: 41c73a4c6a16065c49981bbd59b3e6dbc022b3cc
run:            35714494308 — PASS, 3 / 3 jobs
```

Pinned repo outputs:

```text
jin12 CRM       415 facts / 1,580 relations
agentic Angular 441 facts /   660 relations
kesetovic CRM   488 facts / 2,054 relations
```

All three have zero current supported R7.9/R7.10 full positives. This means:

- **Safety benchmark PASS:** PKC runs successfully and does not manufacture unsupported cross-layer authority.
- **R7.14 NOT PASS:** none of these three unchanged repos naturally demonstrates the exact full positive shape yet.

Zero here is a good safety result, not proof that PKC extracted nothing. R7.14 is the separate usefulness/positive-yield requirement.

Agentic still contains exactly two raw `UpdatedAt` mutations with `runtime-pattern-variable / caller-object-unproven` and zero candidate promotion; mutation-causality remains closed.

## Next action — independent rereview #9

Review exact production:

```text
cd3b1d4b64168c4e95284299c5715b3db0e4da4b
```

Request:

`docs/reviews/2026-09-22-v0.4.7-d-r7.10-rereview-9-request.md`

The reviewer must independently seek a new compile-valid/runtime-valid false-positive, especially across render suppression, tag/attribute lexical boundaries, Angular block nesting, structural directives, exact fact-ID composition and authority downgrade.

If no blocker exists:

```text
mark R7.10 PASS / COMPLETE
→ mark all V0.4.7-D PASS / COMPLETE
→ unlock only V0.4.7-E
→ keep R7.14 NOT PASS and required for E
→ keep V0.5 locked until E completes
```

If a blocker exists, keep E locked and require a regression-first minimum generic repair.
