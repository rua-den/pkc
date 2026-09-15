# Milestones

## V0.1 — C# evidence compiler — COMPLETE

Deterministic Roslyn scanner producing `.pkc/facts.json` with symbols, endpoints, routes, permissions, call evidence and source locations.

### V0.1.1 — Behavior evidence — COMPLETE

Added conditions/guards, throws, state-mutation candidates, call targets, combined routes and publication candidates.

### V0.1.2 — Workflow candidate grouping — COMPLETE

Endpoint-centered traversal groups compact backend evidence into `.pkc/feature-candidates.json`.

## V0.2 — First portable Markdown proof — COMPLETE

`pkc build <repository-path>` turns grounded candidates into canonical knowledge and portable Markdown.

## V0.3 — Frontend static evidence — COMPLETE

React/TypeScript static evidence adds routes, screens, actions, permission guards and API calls and links them to backend behavior.

## V0.4 — Product feature/workflow synthesis — ACTIVE LINE

V0.4 is complete only when generated portable knowledge is sufficiently rich for an AI to answer practical Product Owner questions about observable behavior, business conditions and cross-layer outcomes without re-reading source code.

PKC must preserve three distinct knowledge classes: authoritative business conditions, lower-authority value lineage/provenance, and mutation/causality evidence. Conservative downgrade must prevent false business claims without deleting deterministic causal evidence. See `docs/product-knowledge-contract.md`.

Representative exit questions:

> When is entity X sellable/visible on the web, and what exact conditions must be true for it to appear?

> Where does field X on entity Y come from besides the UI, and what code path can later change it?

### V0.4.1 — Frontend adapter architecture — COMPLETE

Common frontend adapter architecture and canonical UI evidence are accepted.

### V0.4.2 — PokeTrade real-system benchmark — COMPLETE

The runnable `.NET 10 + Angular 22` PokeTrade application is the known-answer behavioral acceptance benchmark.

### V0.4.3 — Analyzer fidelity hardening — COMPLETE

Completed semantic/fallback provenance and frontend/backend analyzer fidelity hardening.

Last accepted tool package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

### V0.4.4 — Loren knowledge readiness — COMPLETE / EXTERNAL REVIEW PASS

Final independent review:

```text
docs/reviews/2026-09-14-v0.4.4-external-rereview-4.md
```

### V0.4.5 — Independent real-repository generalization gate — COMPLETE / INDEPENDENT REVIEW PASS

Accepted benchmark:

```text
repository: jellyfin/jellyfin
pinned commit: 1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

V0.4.5 remains accepted with non-blocking warnings around duplicate HTTP verb extraction, feature-level promotion, and large-pack signal/noise.

### V0.4.6 — Business logic reconstruction — IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING

Purpose: compile deterministic business-decision evidence strongly enough that an AI can answer practical `when`, `why`, `which conditions` and `what makes this visible/eligible` questions from generated knowledge.

Current reviewed production:

```text
868195eff5435cca1c98d4bf6ffd4b18018daf66
fix: require inert clone construction
```

Latest independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-9.md
verdict: FAIL / REOPEN V0.4.6
```

Rereview 8's PASS remains valid for the constructor-effect boundary it reviewed, but the milestone-complete disposition is superseded by a new provider-semantics contradiction.

Current disposition:

```text
B6.1 PASS — exact C# invocation semantic identity; keep closed
B6.2 PASS — conservative configured-item ownership; keep closed
B6.3 PASS — active module-qualified Angular service ownership; keep closed
B6.4 IMPLEMENTED — Queryable authority downgraded pending independent review
```

Accepted B6.4 hardening still covers local/discarded predicate downgrade, transformed return contexts, arbitrary `Select` rejection, identity projection proof, whole-item dependency completeness, safe defensive-clone members, custom setter and initializer effects, callback/comparer pipeline effects, and inert same-type clone construction.

#### Remediated Queryable provider-authority boundary

The prior model retained `System.Linq.Queryable.<operation>` as authority without proving the provider boundary. The implementation now retains it as deterministic evidence while withholding Product Owner authority.

That prior behavior was insufficient because Queryable execution is provider-mediated. `Queryable.Where` creates an expression tree and delegates it to `source.Provider.CreateQuery(...)`; the provider implementation determines the query behavior.

A compile-valid custom `IQueryable<T>` / `IQueryProvider` can retain the expression tree while enumeration ignores the `Where` predicate:

```csharp
private readonly IQueryable<Card> _cards =
    new IgnoringQuery<Card>(
    [
        new Card { IsPublished = false }
    ]);

public IReadOnlyList<Card> GetCards() =>
    _cards
        .Where(card => card.IsPublished)
        .ToList();
```

The returned list can therefore contain `IsPublished == false`, while current PKC may still synthesize:

```text
Includes items from `_cards` only when `card.IsPublished`.
```

Required authority boundary:

```text
exact Queryable target
+ provider/source identity proven
+ provider semantics for the operation proven/trusted
+ observable execution path proven
→ authoritative product rule

otherwise
→ observed-only / no authoritative rule
```

The V0.4.6 implementation downgrades Queryable predicates and rejects Enumerable authority across Queryable pipeline hops. No benchmark/provider-name special cases.

Focused regression: a custom provider that ignores `Where` during enumeration produces observed-only evidence and no authoritative returned-item inclusion rule. Fresh independent review remains pending; V0.4.7 and V0.5 remain locked.

Exact-production gates remain green on `868195eff...`:

```text
CI + PKC tests + WorkPlay + PokeTrade   34964195712 — PASS
pinned Loren                            34964195642 — PASS
Loren-main canary                       34964195717 — PASS
pinned Jellyfin                         34964195689 — PASS
```

Core evidence:

```text
PKC build:       0 warnings / 0 errors
C# tests:        85 / 85 PASS
frontend tests:  13 / 13 PASS
WorkPlay:        PASS
PokeTrade:       PASS
```

Pinned Jellyfin artifact:

```text
artifact id:   10394456737
digest:        sha256:586863e971967be62ec6e0a7763cc90806903621d43ea896813e4cf3b3a2e414
size:          9,016,935 bytes
```

Green automation does not cover the custom-provider contradiction.

Local validation is green: focused authority regressions 7/7, full C# suite 88/88, frontend suite 13/13, and Release build 0 warnings/0 errors. Mutation RED removing both Queryable guards produced expected observed-only versus actual observable; the evidence-retention RED showed the downgraded predicate missing from portable Evidence before `observes-predicate` traversal was added. `DOTNET_ROLL_FORWARD=LatestMajor` was required for SDK 10 MSBuild discovery with the net8 test host. Commit, push, exact-SHA gates, and fresh independent review remain outstanding.

### V0.4.7 — Cross-layer PO question readiness — LOCKED AGAIN UNTIL V0.4.6 RE-PASSES

Purpose: broaden supported direct business-logic patterns into robust cross-layer product-behavior understanding.

Planned target coverage includes frontend visibility predicates, DTO/projection transformations, cross-entity value lineage, snapshot/copy versus dynamic semantics, later overrides/mutation causality, and cross-layer PO-facing explanations.

Do not begin V0.4.7 implementation until the reopened V0.4.6 provider-authority blocker passes fresh independent review.

## V0.5 — Azure DevOps input evidence — LOCKED

Azure DevOps is planned as an additional compiler input for requirement intent, Epic/Feature/PBI history, status and traceability. ADO must not compensate for missing code-derived business logic.

Until ADO is integrated, its absence should be declared as a global knowledge boundary rather than repeated in every feature/workflow file.

V0.5 may start only after the V0.4.x PO-question-readiness exit gate independently passes.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
