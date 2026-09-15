# PKC Status

Last updated: 2026-09-15

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             IMPLEMENTATION GREEN / EXACT-SHA GATES PASS / INDEPENDENT RE-REVIEW PENDING
V0.4.7 cross-layer PO-question readiness         LOCKED
V0.5 Azure DevOps input evidence                 LOCKED
```

Current `main` handoff before this checkpoint:

```text
ec4be388415b9412f5e52367cf03e214498e8f54
docs: preserve Queryable review checkpoint
```

Candidate production under fresh independent review:

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
verdict: FAIL / REOPEN V0.4.6
```

Fresh acceptance request:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10-request.md
review target: c310e893762997f34562a6b3a62dbab2b05c0c93
```

Rereview 8 remains valid for the constructor-effect boundary it accepted. Rereview 9 reopened V0.4.6 only for the separate Queryable provider-authority contradiction. That contradiction is now remediated in `c310e89`; milestone acceptance still requires fresh independent review.

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
B6.4 IMPLEMENTATION GREEN — Queryable provider-authority fix; fresh independent review pending
```

### Prior B6.4 hardening remains accepted

Keep closed absent a concrete contradiction:

- discarded/local predicate downgrade;
- transformed/polarity-changing return-context fail-closed behavior;
- arbitrary `Select` rejection and identity projection proof;
- whole-item predicate dependency completeness;
- safe direct clone-member copies;
- custom setter / initializer effect fail-closed behavior;
- callback/comparer-bearing ordering/equality pipeline fail-closed behavior;
- exact-shape callback-free Enumerable pipeline preservation;
- inert-constructor requirement for same-type defensive clones.

### Remediated boundary — Queryable provider trust

The prior model could treat exact `System.Linq.Queryable.<operation>` identity as sufficient authority even though `IQueryable` execution is provider-mediated.

Rereview 9 demonstrated a compile-valid custom `IQueryable<T>` / `IQueryProvider` whose `CreateQuery` retains the expression while enumeration ignores `Where`. Such a provider can return an unpublished item from:

```csharp
_query.Where(card => card.IsPublished).ToList();
```

while the old authority path could synthesize the false Product Owner rule:

```text
Includes items from `_query` only when `card.IsPublished`.
```

Implementation `c310e89` now applies the conservative generic boundary:

```text
Queryable predicate target
→ retain deterministic predicate evidence
→ businessRuleAuthority = observed-only
→ no authoritative Product Owner rule
```

and:

```text
Enumerable Where path crosses any Queryable pipeline hop
→ fail closed on returned-item authority
→ retain predicate evidence
```

No provider, benchmark, entity, property or fixture name is special-cased. LINQ-to-Objects `AsQueryable()` is also conservatively downgraded because provider identity/semantics are not currently proven.

`FeatureCandidateBuilder` retains `observes-predicate`, so the downgrade does not delete lower-authority evidence required by the product contract.

## Local validation for `c310e89`

Recorded implementation validation:

```text
TDD mutation RED — removing both Queryable guards produced expected observed-only vs actual observable
Evidence-retention RED — downgraded predicate was absent until observes-predicate traversal was retained
Focused WherePipelineCallbackAuthorityRegressionTests: 7 / 7 PASS
Full C# suite: 88 / 88 PASS
Frontend suite: 13 / 13 PASS
Release build: 0 warnings / 0 errors
```

Environment note: `DOTNET_ROLL_FORWARD=LatestMajor` was required on the implementation machine because the net8 test host used SDK 10 MSBuild discovery.

## Exact-SHA automation for `c310e89`

All required current-production gates are green on exact implementation SHA `c310e893762997f34562a6b3a62dbab2b05c0c93`:

```text
CI + PKC tests + WorkPlay + PokeTrade   34990080620 — PASS
pinned Loren                            34990080707 — PASS
Loren-main canary                       34990080551 — PASS
pinned Jellyfin                         34990080546 — PASS
```

CI job-level verification:

```text
Release build                           PASS
dotnet test PKC.sln                     PASS
local tool pack/install                 PASS
WorkPlay end-to-end knowledge           PASS
PokeTrade .NET backend build            PASS
PokeTrade Angular frontend build        PASS
PokeTrade business acceptance           PASS
PokeTrade PKC knowledge compile/verify  PASS
```

Pinned Jellyfin verification:

```text
Build PKC                               PASS
Build pinned Jellyfin                   PASS
Compile product knowledge               PASS
portable bundle/ZIP parity              PASS
raw .pkc leak check                     PASS
src/ leak check                         PASS
```

Pinned Jellyfin artifact:

```text
artifact id: 10405810551
digest:      sha256:8664945310d5fd0da3a0c838b001cc5fa343174410dc1ac6d05b1335e41b0257
size:        9,159,880 bytes
head SHA:    c310e893762997f34562a6b3a62dbab2b05c0c93
```

## Exact next action

Stay in V0.4.6 and perform a fresh independent rereview of exact production SHA `c310e893762997f34562a6b3a62dbab2b05c0c93` using `docs/reviews/2026-09-15-v0.4.6-independent-rereview-10-request.md`.

Do not make another production change merely for output parity. Only reopen implementation if the reviewer finds a concrete compile-valid/behavior-valid contradiction or another acceptance gate fails.

Until independent PASS:

```text
V0.4.7 LOCKED
V0.5 LOCKED
```
