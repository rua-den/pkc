# PKC Status

Last updated: 2026-09-15

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING
V0.4.7 cross-layer PO-question readiness         LOCKED
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.6 is **not complete yet**. The re-review-5 B6.4 projector-effect blocker has been fixed generically and every current automated gate is green on the exact production checkpoint below. A fresh independent review is still required before V0.4.6 may be marked complete.

Production checkpoint for independent review:

```text
18a1f1d1ef551833d859f23ea2e92dd548a6a81d
fix: fail closed on unsafe same-type projector effects
```

Latest completed independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-5.md
verdict: FAIL / FIX REQUIRED
```

Next review request:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-6-request.md
```

V0.4.3 remains the last accepted tool package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

## Product contract governing V0.4.x

PKC must generate portable knowledge rich enough for an AI to answer practical Product Owner questions about observable behavior, business conditions, value origin, mutation causality and cross-layer outcomes without re-reading source code.

The permanent guardrail is documented in `docs/product-knowledge-contract.md`:

```text
business conditions
+ value lineage / provenance
+ mutation / causality
```

No deterministic proof means no authoritative product claim. However, downgrading product-rule authority must **not** erase deterministic lower-authority evidence that can answer questions such as:

```text
Where did Service.A come from besides the UI?
Why did this status/value change?
What can override this value after creation?
```

Until Azure DevOps is actually integrated, its absence belongs in the global knowledge boundary/index. Do not repeat ADO/product-intent placeholders in every feature/workflow Markdown file.

## V0.4.6 disposition

### B6.1 — PASS / keep closed

Exact C# predicate authority is re-resolved from exact invocation syntax identity. Same-line custom and genuine LINQ invocations cannot borrow authority.

### B6.2 — PASS / keep closed

Configured-item ownership remains conservative; nested property object initializers are not promoted as direct collection items.

### B6.3 — PASS / keep closed

Angular service ownership remains module-qualified and requires lexically active relative import evidence. Ambiguity/unresolved ownership is omitted rather than guessed.

### B6.4 — IMPLEMENTATION GREEN / independent acceptance pending

Independent re-review 5 found that a same-type projector could directly copy a predicate-relevant member and then invalidate that copied state through another initializer target's custom setter.

Checkpoint `18a1f1d1ef551833d859f23ea2e92dd548a6a81d` closes that authority path conservatively.

Same-type method-group projection may now preserve `Where` authority only when all of the following are proven:

```text
predicate dependency set is complete
AND every initializer expression is a simple assignment
AND every output target is a direct stored non-static field or auto-property
AND every initializer assignment is an exact same-member source → output copy
AND every predicate-required member is directly copied
→ authoritative returned-item predicate may be retained

otherwise
→ downgrade / no authoritative Product Owner inclusion rule
```

This intentionally rejects custom setters, nested/unmodeled initializer effects, helper/constant/rewritten right-hand sides and other unproven projector effects instead of attempting arbitrary C# side-effect verification.

Focused regression:

```text
Later_custom_setter_that_invalidates_predicate_state_downgrades_projection_authority
```

The regression uses a behavior-valid source initializer ordering so the source item actually reaches `Where` with `IsPublished == true`; the clone then invalidates copied state through the later custom setter. The re-review-5 document remains unchanged as the audit record.

The existing positive PokeTrade-style direct-member defensive-clone regression remains green.

## Exact automation for production checkpoint

All push-triggered gates for `18a1f1d1ef551833d859f23ea2e92dd548a6a81d` are green:

```text
CI + PKC tests + WorkPlay + PokeTrade   34954590262 — PASS
pinned Loren                            34954590188 — PASS
Loren-main canary                       34954590239 — PASS
pinned Jellyfin                         34954590155 — PASS
portable parity / provenance / no-leak  PASS inside Jellyfin run
```

Exact CI evidence:

```text
PKC build:                         0 warnings / 0 errors
C# tests:                          77 / 77 PASS
frontend tests:                    13 / 13 PASS
tool pack/install:                 PASS
WorkPlay:                          PASS
PokeTrade backend build:           PASS
PokeTrade Angular build:           PASS
PokeTrade live business branches:  PASS
PokeTrade generated knowledge:     PASS
```

Pinned Jellyfin evidence:

```text
jellyfin/jellyfin @ 1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
source build:            PASS, 0 warnings / 0 errors
facts:                   43,363
relations:               195,314
workflow candidates:     386
product features:        116
canonical Markdown:      504 files
analysis mode:           project-semantic 43,363 / 43,363
bundle canonical parity: PASS
ZIP file-set parity:     PASS
ZIP byte parity:         PASS
raw .pkc leak:           none
src/ source-tree leak:   none
artifact id:             10390258392
artifact digest:         sha256:6ba875d99cf64141267bb13811485cc025eabc87723beaa9f3a46a985860b497
artifact size:           9,016,989 bytes
```

## Validation note

The current coding sandbox has no local `dotnet` executable, so local runtime red/green execution could not be claimed. The focused regression, generic fix and complete diff were reviewed before one production push; final runtime validation then ran on the exact pushed SHA through the repository gates above.

## Exact next action

Do not change production code unless fresh independent review finds a concrete contradiction.

Request an independent V0.4.6 re-review of exact production checkpoint:

```text
18a1f1d1ef551833d859f23ea2e92dd548a6a81d
```

If and only if that independent review returns PASS:

```text
mark V0.4.6 COMPLETE
unlock V0.4.7 as next/current milestone
keep V0.5 Azure DevOps locked
```

Until then:

```text
V0.4.7 LOCKED
V0.5 LOCKED
```
