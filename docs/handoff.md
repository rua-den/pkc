# PKC Handoff

Use this file when continuing PKC in another coding or independent-review thread.

## Product contract

PKC is a deterministic Product/System Knowledge Compiler. A Product Owner should be able to hand generated portable knowledge to an AI and ask practical product/system questions without the AI re-reading source code.

Keep the architecture:

```text
source inputs
→ deterministic analyzers/adapters
→ evidence/facts
→ feature/workflow/business-decision candidates
→ knowledge synthesis
→ canonical model
→ portable rendering
```

Do not add direct source-to-freeform-AI generation.

Permanent guardrail: `docs/product-knowledge-contract.md`.

Keep distinct and retain:

```text
business conditions
value lineage / provenance
mutation / causality
```

Conservative downgrade of product-rule authority must not erase deterministic lower-authority evidence.

## Current state

```text
V0.4.4  Loren knowledge readiness              PASS / COMPLETE
V0.4.5  Jellyfin generalization               PASS / COMPLETE
V0.4.6  business logic reconstruction         PASS / COMPLETE
V0.4.7  cross-layer PO-question readiness     CURRENT / NEXT MILESTONE
V0.5    Azure DevOps input evidence           LOCKED
```

Accepted V0.4.6 production checkpoint:

```text
868195eff5435cca1c98d4bf6ffd4b18018daf66
fix: require inert clone construction
```

Latest completed independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-8.md
PASS / COMPLETE on production 868195eff5435cca1c98d4bf6ffd4b18018daf66
```

V0.4.6 is closed. Do not reopen its accepted blockers without a concrete contradiction.

## Accepted V0.4.6 scope

```text
B6.1 PASS / keep closed
B6.2 PASS / keep closed
B6.3 PASS / keep closed
B6.4 PASS / keep closed
```

Accepted B6.4 hardening includes:

1. discarded/local predicates do not become observable rules merely because a LINQ call exists;
2. transformed/polarity-changing return contexts for `Any`, `All`, `First*`, `Single*` fail closed unless modeled;
3. arbitrary `Select` is not an unconditional preserving operation after `Where`;
4. direct identity `Select(card => card)` is proven by symbol identity;
5. unsupported whole-item predicate dependencies cause conservative downgrade;
6. same-type method-group projection requires a closed item type and direct same-member initializer copies;
7. custom setter / nested / rewritten initializer effects fail closed;
8. callback/comparer-bearing pipeline operations are not trusted merely from LINQ method identity;
9. callback-free pipeline preservation is limited to an audited exact-shape subset;
10. same-type projector construction must itself be proven inert before earlier `Where` authority is retained.

The callback/comparer pipeline blocker from rereview 6 remains closed. `OrderBy*`, `ThenBy*`, `Distinct`, and `ToHashSet` do not automatically borrow earlier `Where` authority.

## Accepted constructor-effect boundary

Rereview 7 demonstrated that a same-type projector could call a copy constructor which mutates the source item after `Where` passed and before initializer values were copied.

Production `868195eff...` closes the generic construction gap conservatively. Same-type method-group projection can preserve authority only when construction is proven inert:

```text
zero constructor args
+ exact constructor symbol on projected type
+ compiler-generated implicit zero-arg constructor
+ no non-object base-constructor path for classes
+ no instance field/event/property initializer code
+ existing safe direct same-member initializer proof
+ complete predicate-member coverage
→ preservation may continue

anything unproven
→ fail closed / observed-only
```

Regression coverage in `tests/Pkc.CSharp.Tests/ProjectionConstructionAuthorityRegressionTests.cs` covers:

1. source-mutating copy constructor;
2. user-defined parameterless constructor;
3. implicit constructor with instance initializer;
4. implicit constructor with effectful base constructor.

Independent rereview 8 additionally challenged constructor overloads, optional/`params` constructors callable with zero supplied arguments, target-typed `new`, explicit `new Type()`, parenthesized creation, partial type declarations, inheritance/base constructors and semantic candidate fallback. No compile-valid/behavior-valid bypass was found.

The existing positive implicit inert defensive-clone behavior remains accepted. No benchmark-specific exception exists.

Fail-closed behavior retains predicate facts as observed-only evidence, preserving lower-authority lineage/mutation/causal evidence rather than deleting it.

## Exact accepted-production gates

All gates are green on exact SHA `868195eff5435cca1c98d4bf6ffd4b18018daf66`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   34964195712 — PASS
pinned Loren                                34964195642 — PASS
Loren-main canary                           34964195717 — PASS
pinned Jellyfin                             34964195689 — PASS
```

Core CI:

```text
PKC build:                 0 warnings / 0 errors
C# tests:                  85 / 85 PASS
frontend tests:            13 / 13 PASS
WorkPlay:                  PASS
PokeTrade backend:         0 warnings / 0 errors
PokeTrade Angular build:   PASS
PokeTrade live acceptance: PASS
PokeTrade knowledge gate:  PASS
```

Pinned Jellyfin:

```text
commit:               1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
source build:          0 warnings / 0 errors
facts:                 43,363
relations:             195,314
workflow candidates:   386
product features:      116
canonical Markdown:    504
project-semantic:      43,363 / 43,363
artifact id:           10394456737
digest:                sha256:586863e971967be62ec6e0a7763cc90806903621d43ea896813e4cf3b3a2e414
size:                  9,016,935 bytes
```

Portable bundle and ZIP parity are exact; no raw `.pkc` and no `src/` source tree leaks into the portable ZIP.

The reviewer environment did not have `dotnet`; no local runtime rerun is claimed. Exact-production automation above is the recorded runtime verification.

## Next thread

V0.4.7 is now the current / next milestone. This handoff does not start its implementation.

Read in order:

1. `docs/status.md`
2. `docs/handoff.md`
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-8.md`

Then continue from the current `main` HEAD and define/execute only the V0.4.7 cross-layer PO-question-readiness scope and acceptance criteria. Preserve all accepted V0.4.6 semantic-authority and evidence-retention guardrails.

Keep:

```text
V0.4.7 CURRENT / NEXT MILESTONE
V0.5 LOCKED
```

Do not begin Azure DevOps ingestion until the V0.4.x PO-question-readiness exit gate allows it.
