# PKC Status

Last updated: 2026-09-24

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                   PASS / COMPLETE
V0.4.5 real-repository generalization              PASS / COMPLETE
V0.4.6 business logic reconstruction               PASS / COMPLETE
V0.4.7-A origin and copy timing                    PASS / COMPLETE
V0.4.7-B computation and later change              PASS / COMPLETE
V0.4.7-C backend to API                            PASS / COMPLETE
V0.4.7-D / R7.9 API to rendered value              PASS / COMPLETE
V0.4.7-D / R7.10 joint visibility                  REPAIRED / EXACT-SHA GATES PASS / PENDING REREVIEW #18
V0.4.7-D overall                                   OPEN / PENDING INDEPENDENT REREVIEW #18
V0.4.7-E0 repository discovery + bounded run       LOCKED behind D
V0.4.7-E1 remaining product-value repairs          LOCKED behind E0
V0.4.7-E2 final product acceptance / R7.14         LOCKED behind E1
continuous update/diff                             LOCKED
V0.5 Azure DevOps input evidence                   LOCKED
```

R7.10 has been repaired after independent rereview #17, but D remains an independent-review gate. This implementation continuation MUST NOT self-certify the repair or unlock E0/RD1.

## Exact production candidate

```text
a67abfa4caded980bd8abec317598afd0ea16a42
fix: fail closed unsupported Angular alias expressions
```

Repair chain from the rereview #17 docs checkpoint:

```text
6435e3b04cba7cbb4ff1fad31c859eb8fee69b02
fix: fail closed semicolonless Angular import aliases

5613477cf225d852501e1d02bb5562a60ea20e61
fix: cover TypeScript ASI line terminators

a67abfa4caded980bd8abec317598afd0ea16a42
fix: fail closed unsupported Angular alias expressions
```

The follow-up commits were justified by adversarial TypeScript 5.8.3 evidence before acceptance, not speculative CI debugging: first lone CR/U+2028/U+2029 ASI, then a line terminator inside a block comment. The final repair stops trying to model partial ASI grammar and conservatively fails closed from the unsupported alias/expression identifier prefix.

## Net change

```text
src/Pkc.Frontend/AngularUnsupportedComponentImportIndirectionAuthorityFilter.cs
tests/Pkc.CSharp.Tests/AngularSemicolonlessComponentImportIndirectionAuthorityRegressionTests.cs
```

Net diff from rereview #17 docs checkpoint:

```text
1 production regex line changed
1 focused regression file added
no unrelated production refactor
```

The guard recognizes unsupported alias/indirection from:

```text
const|let|var <identifier> = <identifier>
```

without requiring a particular source terminator. If that prefix continues into a call/member/binary or other expression shape that PKC has not proven as supported component-import topology, conservative fail-close is intentional. Ordinary supported direct/local-array framework imports remain positive-regression protected.

Focused coverage includes semicolonless LF, duplicate identifiers across scopes, lone CR, U+2028, U+2029 and a line terminator inside a block comment. Existing semicolon/default-reexport negative coverage and ordinary supported framework-array positive coverage remain.

## Verification

Local execution environment limitation:

```text
.NET SDK                              unavailable
shell external DNS/network            unavailable
TypeScript 5.8.3 / Node               available
```

No repository-local `.NET` test claim is made.

Local TypeScript 5.8.3 evidence:

```text
LF ASI                                  PASS
CR ASI                                  PASS
U+2028 ASI                              PASS
U+2029 ASI                              PASS
block-comment-contained line terminator PASS
same block comment without line break   compile FAIL
```

Exact-SHA clean-environment gates:

```text
CI / full tests / WorkPlay / PokeTrade   35939834957 PASS
Loren pinned/external                    35939834993 PASS
Loren-main canary                        35939834966 PASS
Jellyfin parity                          35939834845 PASS
```

These gates are regression evidence only; they do not replace independent rereview #18.

Fresh review request:

```text
docs/reviews/2026-09-24-v0.4.7-d-r7.10-rereview-18-request.md
```

## Demo-critical roadmap

Only after independent rereview #18 PASS explicitly closes D:

```text
E0 Repository Discovery + bounded run readiness
RD1 inventory + safe exclusion
→ RD2 application boundaries + ownership
→ RD3 vendor/custom frontend classification
→ RD4 runtime/plugin provenance
→ RD5 deterministic ScanPlan
→ RD6 scoped/bounded semantic execution
→ RD7 coverage + observability + plan-only inspection
→ RD8 private large-repo validation
→ prove pkc run can practically produce .pkc/workspace
→ E1 remaining semantic richness
→ E2 final product acceptance
```

The private mixed legacy repository observation (~9 GB RAM before useful completion) remains evidence of a real product-operability problem, not a universal hard threshold. Scope reduction/discovery is the first demo priority once D closes.

## E0 invariants — prepared, still locked

- discovery precedes expensive semantic scanning;
- repository is a graph, not one homogeneous root;
- application boundaries matter;
- source role and scan mode are separate concepts;
- only strongly proven generated/restorable areas may auto-exclude;
- name-only heuristics never create exclusion authority;
- THIRD_PARTY_RUNTIME is not automatically irrelevant;
- modified vendor areas require narrow first-party carve-outs;
- UNKNOWN remains valid;
- tests are evidence, not production authority;
- runtime/plugin edges require deterministic provenance;
- similar names never create dependency edges;
- discovery remains shallow and bounded;
- private source/config values never leak into portable output.

Fix #4 remains parked off-main:

```text
branch  fix/product-value-construction-state
commit  35c8e5c5f856e15568aa963bb2d76268008c5570
```

## Exact next action

```text
fresh independent rereview #18 of exact
a67abfa4caded980bd8abec317598afd0ea16a42
```

If PASS:

```text
mark R7.10 + V0.4.7-D PASS / COMPLETE
→ explicitly unlock E0/RD1
→ start RD1 only, regression-first
```

If FAIL, keep later checkpoints locked and return to the minimum generic R7.10 repair.
