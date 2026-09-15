# PKC Handoff

Use this file when continuing PKC in another coding or independent-review thread.

## Read first

1. `docs/status.md`
2. this handoff
3. `docs/milestones.md`
4. `docs/product-knowledge-contract.md`
5. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-9.md`
6. `docs/reviews/2026-09-15-v0.4.6-queryable-direction-checkpoint.md`
7. `docs/reviews/2026-09-15-v0.4.6-independent-rereview-10-request.md`

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
V0.4.6  business logic reconstruction         IMPLEMENTATION GREEN / EXACT-SHA GATES PASS / INDEPENDENT RE-REVIEW PENDING
V0.4.7  cross-layer PO-question readiness     LOCKED
V0.5    Azure DevOps input evidence           LOCKED
```

Candidate production for the next independent review:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Last independently accepted production before rereview 9:

```text
868195eff5435cca1c98d4bf6ffd4b18018daf66
fix: require inert clone construction
```

Latest independent review result:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-9.md
FAIL / REOPEN V0.4.6
```

Fresh acceptance request:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10-request.md
review exact production SHA c310e893762997f34562a6b3a62dbab2b05c0c93
```

Rereview 8 remains accepted for its constructor-effect analysis. Rereview 9 found a separate Queryable provider-semantics contradiction; `c310e89` remediates that blocker. Do not reopen older closed boundaries without a new concrete contradiction.

## Closed scope

Keep closed unless fresh evidence contradicts it:

```text
B6.1 PASS — exact C# invocation semantic identity
B6.2 PASS — conservative configured-item ownership
B6.3 PASS — module-qualified Angular service ownership
```

Previously hardened B6.4 boundaries also remain accepted:

1. discarded/local predicates downgrade;
2. transformed/polarity-changing `Any`/`All`/`First*`/`Single*` contexts fail closed;
3. arbitrary `Select` is not preserving by default;
4. identity `Select(card => card)` uses symbol identity;
5. whole-item/unmodeled predicate dependencies fail closed;
6. direct defensive clones require stored same-member copies;
7. custom setters, nested writes and rewritten initializer effects fail closed;
8. ordering/equality callback/comparer paths do not preserve authority merely from LINQ target identity;
9. callback-free Enumerable pipeline preservation is exact-shape and conservative;
10. same-type defensive clone construction must be deterministically inert.

## B6.4 Queryable provider authority — implemented, review pending

Rereview 9 proved that exact `System.Linq.Queryable.Where` identity does not prove runtime predicate semantics. `Queryable.Where` builds an expression tree and delegates to `IQueryProvider`; a valid custom provider may retain that expression and still enumerate unfiltered items.

The required generic boundary is:

```text
exact Queryable operation
+ source/provider identity proven
+ provider semantics for that operation proven
+ observable execution path proven
→ authoritative Product Owner rule

otherwise
→ observed-only / no authoritative product rule
```

Current implementation intentionally takes the conservative branch because provider identity/semantics are not proven.

### Production changes in `c310e89`

`CSharpBusinessPredicateAuthorityFilter`:

- exact `System.Linq.Queryable` predicate targets receive no observable Product Owner authority;
- any `System.Linq.Queryable.*` pipeline hop causes an Enumerable `Where` returned-item authority proof to fail closed;
- downgraded predicate facts remain with `businessRuleAuthority=observed-only` and `observableEffectResolution=not-proven`;
- downgraded `contains-condition` relations become `observes-predicate` rather than deleting the predicate.

`FeatureCandidateBuilder`:

- `observes-predicate` is retained as behavior evidence so lower-authority evidence survives candidate construction.

Regression coverage:

- a custom `IQueryable<T>` / `IQueryProvider` that ignores `Where` is observed-only and does not synthesize the false inclusion rule;
- the in-process provider runtime regression proves the provider can return an unpublished item;
- Enumerable `Where` followed by a Queryable hop is downgraded;
- pure callback-free Enumerable filtering remains authoritative.

No PokeTrade, Loren, Jellyfin, EF, provider-brand, entity-name, property-name or fixture-text exception exists.

## Local validation for `c310e89`

```text
TDD mutation RED: removing both Queryable guards restored the bad observable authority
Evidence-retention RED: predicate disappeared until observes-predicate traversal was added
Focused WherePipelineCallbackAuthorityRegressionTests: 7 / 7 PASS
Full C# suite: 88 / 88 PASS
Frontend suite: 13 / 13 PASS
Release build: 0 warnings / 0 errors
```

Implementation-machine note: `DOTNET_ROLL_FORWARD=LatestMajor` was required for net8 test-host/MSBuild discovery with the installed SDK 10 toolchain.

## Exact-SHA gates for `c310e89`

All required remote gates are now established on the exact implementation SHA:

```text
CI + full PKC tests + WorkPlay + PokeTrade   34990080620 — PASS
pinned Loren                                34990080707 — PASS
Loren-main canary                           34990080551 — PASS
pinned Jellyfin                             34990080546 — PASS
```

CI job-level evidence:

```text
Release build                           PASS
dotnet test PKC.sln                     PASS
local tool pack/install                 PASS
WorkPlay end-to-end knowledge           PASS
PokeTrade .NET backend build            PASS
PokeTrade Angular frontend build        PASS
PokeTrade business acceptance           PASS
PokeTrade PKC compile/verify             PASS
```

Pinned Jellyfin gate confirms:

```text
PKC build                               PASS
pinned Jellyfin build                   PASS
knowledge compile                       PASS
portable bundle/ZIP parity              PASS
no raw .pkc in portable ZIP             PASS
no src/ leakage in portable ZIP         PASS
```

Artifact:

```text
id:      10405810551
digest:  sha256:8664945310d5fd0da3a0c838b001cc5fa343174410dc1ac6d05b1335e41b0257
size:    9,159,880 bytes
head:    c310e893762997f34562a6b3a62dbab2b05c0c93
```

## Exact next action

Do **not** write more production code merely because V0.4.6 is not yet marked complete.

The implementation and exact-SHA gates are green. The next required action is a fresh independent rereview of `c310e893762997f34562a6b3a62dbab2b05c0c93` using the rereview-10 request.

If the review PASSes:

```text
B6.4 PASS — close
V0.4.6 PASS / COMPLETE
V0.4.7 becomes CURRENT / NEXT
V0.5 remains LOCKED
```

If the review FAILs, require the smallest compile-valid/behavior-valid counterexample, keep V0.4.7/V0.5 locked, and return to regression-first implementation only for that concrete blocker.

Do not self-approve V0.4.6 in a coding thread.
