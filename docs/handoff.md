# PKC Handoff

Last updated: 2026-09-24

This handoff defines the next safe execution order after the R7.10 semicolonless/ASI scalar-alias repair.

## Read first

1. root `CLAUDE.md`
2. `AGENTS.md`
3. `docs/status.md`
4. this handoff
5. `docs/milestones.md`
6. `docs/product-knowledge-contract.md`
7. `docs/v0.4.7-acceptance-plan.md`
8. `docs/reviews/2026-09-24-v0.4.7-d-r7.10-independent-rereview-17.md`
9. `docs/reviews/2026-09-24-v0.4.7-d-r7.10-rereview-18-request.md`
10. `docs/plans/2026-09-24-demo-critical-sequential-execution-plan.md`
11. `docs/plans/2026-09-24-repository-discovery-scan-planning-plan.md`
12. `docs/reviews/2026-09-24-repository-discovery-scan-planning-self-review.md`
13. `docs/benchmarks/product-value-benchmark-protocol.md`

Then inspect current `main`, recent commits, exact production diff, regression coverage and gate evidence. Never reset to an older SHA merely because this handoff names one.

## Current formal state

```text
A/B/C                    PASS / COMPLETE
R7.9                     PASS / COMPLETE
mutation-causality       PASS / CLOSED
R7.10                    REPAIRED / EXACT-SHA GATES PASS / PENDING REREVIEW #18
V0.4.7-D                 OPEN / PENDING INDEPENDENT REREVIEW #18
V0.4.7-E0                LOCKED behind D
E1 semantic richness     LOCKED behind E0
E2 product acceptance    LOCKED behind E1
R7.14                    NOT PASS / REQUIRED FOR E2
V0.5                     LOCKED
```

This implementation continuation must not self-certify its own repair. A fresh independent reviewer is the next formal gate.

## Exact production to review

```text
a67abfa4caded980bd8abec317598afd0ea16a42
fix: fail closed unsupported Angular alias expressions
```

Repair commits above rereview #17 docs checkpoint:

```text
6435e3b04cba7cbb4ff1fad31c859eb8fee69b02
fix: fail closed semicolonless Angular import aliases

5613477cf225d852501e1d02bb5562a60ea20e61
fix: cover TypeScript ASI line terminators

a67abfa4caded980bd8abec317598afd0ea16a42
fix: fail closed unsupported Angular alias expressions
```

Net production/test scope from `9e8d2ea94f6457a6fd2ef38d3dd88b8fa7c1b3ad`:

```text
src/Pkc.Frontend/AngularUnsupportedComponentImportIndirectionAuthorityFilter.cs
tests/Pkc.CSharp.Tests/AngularSemicolonlessComponentImportIndirectionAuthorityRegressionTests.cs
```

No Repository Discovery code, Fix #4 code or unrelated semantic refactor is included.

## Why the repair converged on conservative prefix fail-close

Rereview #17 proved optional semicolon omission is compile-valid TypeScript and bypassed the old guard.

Before acceptance, TypeScript 5.8.3 adversarial compilation proved further valid ASI forms: lone CR, U+2028, U+2029 and a line terminator inside a block comment. The same block comment without a line break does not compile at that boundary.

Rather than keep extending a partial ASI lexer inside a regex, final production recognizes unsupported alias/indirection from:

```text
const|let|var <identifier> = <identifier>
```

without requiring a particular source terminator.

If the identifier prefix continues into a call/member/binary or other expression shape PKC has not proven as supported component-import topology, conservative fail-close is intentional at this authority boundary. Ordinary supported direct/local-array framework imports remain protected by positive regressions.

Focused new regressions cover LF semicolonless aliases, duplicate identifiers across scopes, lone CR, U+2028, U+2029 and block-comment-contained line terminators. Existing semicolon/default-reexport fail-close and supported framework-array positive cases remain.

## Verification

Implementation environment limitation:

```text
.NET SDK                    unavailable
shell external DNS/network  unavailable
TypeScript 5.8.3 / Node     available
```

No claim is made that `.NET` tests were run locally.

Local deterministic/oracle evidence:

```text
tsc 5.8.3 LF ASI                         PASS
tsc 5.8.3 CR ASI                         PASS
tsc 5.8.3 U+2028                         PASS
tsc 5.8.3 U+2029                         PASS
tsc 5.8.3 block-comment line terminator  PASS
same block comment without line break     compile FAIL
```

Exact-SHA clean-environment gates for `a67abfa...`:

```text
CI / full tests / WorkPlay / PokeTrade   35939834957 PASS
Loren pinned/external                    35939834993 PASS
Loren-main canary                        35939834966 PASS
Jellyfin parity                          35939834845 PASS
```

CI is final verification here because a working local .NET runtime could not be obtained in this environment. Do not rewrite Actions evidence as local execution.

## Fresh independent rereview #18

Review request:

```text
docs/reviews/2026-09-24-v0.4.7-d-r7.10-rereview-18-request.md
```

Reviewer must independently search for a new compile-valid/runtime-valid false or over-authoritative R7.10 path, not merely replay the new regression.

High-value seams include ASI/trivia/comments, duplicate scopes, expression continuations, local/barrel/default/namespace imports, package/link layouts, exact component association, projection/selector/control-flow authority and lower-authority evidence preservation.

An unsupported/ambiguous shape that conservatively fails closed is not a blocker.

## Demo-critical sequence after D passes

Only after a fresh independent PASS explicitly closes D:

```text
E0 — Repository Discovery + bounded run readiness
RD1 inventory + safe exclusion
→ RD2 application boundaries + ownership
→ RD3 vendor/custom frontend classification
→ RD4 runtime/plugin provenance
→ RD5 deterministic ScanPlan
→ RD6 scoped/bounded semantic execution
→ RD7 coverage + observability + plan-only inspection
→ RD8 private large-repo validation
→ prove pkc run produces the intended .pkc/workspace practically

E1 — Remaining product-value repairs
construction/default/computation
→ semantic integration side effects
→ feature-summary rule fidelity

E2 — Product acceptance
R7.14 positive unchanged-real-project yield
→ Level-2 known-answer benchmark
→ portable workspace acceptance
```

Production checkpoints remain sequential. Independent reading/source inspection/non-conflicting verification may be parallelized only inside the current checkpoint.

## E0 technical principles — prepared, still locked

- discovery MUST precede expensive semantic scans;
- repository is an application/component graph, not a homogeneous root;
- source role and scan mode are separate concepts;
- generated/restorable auto-exclusion needs strong evidence;
- name-only heuristics never create exclusion authority;
- third-party runtime assets remain visible;
- modified vendor code gets only narrow deterministic first-party carve-outs;
- tests are evidence, not production authority;
- runtime/plugin edges require provenance;
- naming similarity is never dependency proof;
- UNKNOWN is valid;
- discovery stays shallow/bounded;
- scan decisions are stable, inspectable and privacy-safe;
- plan-only inspection exists before expensive semantic execution;
- coverage cannot falsely claim READY.

## Fix #4 — keep parked

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
```

Do not merge before D passes and E0 completes.

## Exact next action

```text
fresh independent rereview #18 of exact
a67abfa4caded980bd8abec317598afd0ea16a42
```

If PASS, mark R7.10/D PASS / COMPLETE, unlock E0/RD1 explicitly, and start RD1 only.

If FAIL, record the exact proof-boundary counterexample and return to the minimum generic R7.10 repair.

Do not start RD1 while this independent gate is still open.
