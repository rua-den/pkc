# PKC Status

Last updated: 2026-09-15

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             PASS / COMPLETE
V0.4.7 cross-layer PO-question readiness         CURRENT / NEXT MILESTONE
V0.5 Azure DevOps input evidence                 LOCKED
```

Accepted V0.4.6 production checkpoint:

```text
868195eff5435cca1c98d4bf6ffd4b18018daf66
fix: require inert clone construction
```

Latest completed independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-8.md
verdict: PASS / COMPLETE
reviewed production: 868195eff5435cca1c98d4bf6ffd4b18018daf66
```

V0.4.3 remains the last accepted tool package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

## Product contract governing V0.4.x

PKC must generate portable knowledge rich enough for an AI to answer practical Product Owner questions about observable behavior, business conditions, value origin, mutation causality and cross-layer outcomes without re-reading source code.

Permanent guardrail: `docs/product-knowledge-contract.md`.

Keep distinct and retain:

```text
business conditions
value lineage / provenance
mutation / causality
```

No deterministic proof means no authoritative product claim. Downgrading rule authority must not erase deterministic lower-authority causal/value-origin evidence.

## V0.4.6 disposition

```text
B6.1 PASS — exact C# invocation semantic identity; keep closed
B6.2 PASS — conservative configured-item ownership; keep closed
B6.3 PASS — module-qualified Angular service ownership; keep closed
B6.4 PASS — constructor-effect authority gap closed; keep closed
```

V0.4.6 is complete after independent rereview 8 accepted exact production `868195eff...`.

### Closed callback/comparer pipeline blocker

Production `eb0903ef...` replaced the broad unconditional `Where` pipeline allowlist with an exact-shape callback-free safe subset. `OrderBy*`, `ThenBy*`, `Distinct`, and `ToHashSet` no longer preserve authoritative `Where` semantics merely from LINQ method identity.

Independent rereviews 7 and 8 found no concrete contradiction in that boundary. Keep it closed absent new evidence.

### Accepted constructor-effect checkpoint

Rereview 7 found that the same-type defensive-clone proof validated initializer writes but did not validate object-constructor effects. Production `868195eff...` requires construction itself to be deterministically inert before preserving an earlier authoritative `Where` predicate.

Accepted conservative boundary:

```text
same closed item type
+ zero constructor arguments
+ constructor resolves exactly to the projected type
+ compiler-generated implicit zero-argument constructor
+ no unproven custom base-constructor path
+ no instance field/event/property initializer code
+ every clone initializer write proven safe direct same-member copy
+ every predicate-relevant member copied
→ may preserve authoritative Where semantics

otherwise
→ observed-only / no authoritative inclusion rule
```

Regressions cover:

```text
source-mutating copy constructor
user-defined parameterless constructor
implicit constructor with instance initializer
implicit constructor with effectful base constructor
```

Independent rereview 8 additionally challenged constructor overloads, optional/`params` zero-argument calls, target-typed `new`, explicit `new Type()`, parentheses, partial declarations, inheritance/base construction and semantic-resolution fallback. No compile-valid/behavior-valid bypass was found.

The existing positive implicit inert defensive-clone regression remains green. The implementation is generic and contains no benchmark-specific exception.

Fail-closed construction retains the predicate as observed-only evidence rather than deleting deterministic lower-authority evidence, preserving the permanent product-knowledge contract.

Local limitation: the reviewer environment had no `dotnet`, so no local runtime test is claimed. Exact-production CI is the runtime verification layer.

## Exact automation for accepted production `868195eff...`

All exact-SHA gates are green:

```text
CI + PKC tests + WorkPlay + PokeTrade   34964195712 — PASS
pinned Loren                            34964195642 — PASS
Loren-main canary                       34964195717 — PASS
pinned Jellyfin                         34964195689 — PASS
```

Core CI:

```text
PKC build:       0 warnings / 0 errors
C# tests:        85 / 85 PASS
frontend tests:  13 / 13 PASS
WorkPlay:        PASS
PokeTrade:       PASS
```

PokeTrade backend and Angular build, live business-branch acceptance, and generated-knowledge known-answer checks all pass.

Pinned Jellyfin:

```text
pinned commit:        1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
source build:         0 warnings / 0 errors
facts:                43,363
relations:            195,314
workflow candidates:  386
product features:     116
canonical Markdown:   504 files
analysis mode:        project-semantic 43,363 / 43,363
artifact id:          10394456737
artifact digest:      sha256:586863e971967be62ec6e0a7763cc90806903621d43ea896813e4cf3b3a2e414
artifact size:        9,016,935 bytes
```

Portable validation passes:

```text
PKC_KNOWLEDGE.md contains every canonical Markdown file verbatim
portable ZIP file set == canonical Markdown file set
portable ZIP bytes == canonical Markdown bytes
no raw .pkc leak in portable ZIP
no src/ source-tree leak in portable ZIP
```

## Exact next action

V0.4.6 is closed. V0.4.7 is now the current / next milestone.

Start V0.4.7 only from the current `main` HEAD and preserve all accepted V0.4.6 authority/retention guardrails. Define and execute the V0.4.7 cross-layer PO-question-readiness acceptance work before considering any V0.5 Azure DevOps ingestion.

```text
V0.4.7 CURRENT / NEXT MILESTONE
V0.5 LOCKED
```
