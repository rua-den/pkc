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

Representative exit question:

> When is entity X sellable/visible on the web, and what exact conditions must be true for it to appear?

Required evidence includes deterministic predicates, configured/static values, data flow, frontend presentation where source can prove them, and explicit unknowns where runtime state cannot be proven statically.

### V0.4.1 — Frontend adapter architecture — COMPLETE

- `IFrontendAdapter` contract
- framework-agnostic `FrontendScanner`
- React and Angular adapters emit common canonical `ui-*` facts
- generic UI-action → API relation linking

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

Accepted baseline includes PokeTrade, pinned Loren, Loren-main and portable handoff parity, with conservative cross-stack semantic authority.

### V0.4.5 — Independent real-repository generalization gate — COMPLETE / INDEPENDENT REVIEW PASS

Accepted benchmark:

```text
repository: jellyfin/jellyfin
pinned commit: 1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

Records:

```text
docs/trials/2026-09-14-v0.4.5-jellyfin.md
docs/trials/2026-09-14-v0.4.5-jellyfin-crosscheck.md
docs/reviews/2026-09-14-v0.4.5-v0.4.6-independent-review.md
```

V0.4.5 remains accepted with non-blocking warnings around duplicate HTTP verb extraction, feature-level promotion, and large-pack signal/noise.

### V0.4.6 — Business logic reconstruction — CURRENT / IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING

Purpose: compile deterministic business-decision evidence strongly enough that an AI can answer practical `when`, `why`, `which conditions` and `what makes this visible/eligible` questions from generated knowledge.

Latest completed independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-4.md
reviewed production: a636172ea8575f46d51e503d1ba7ad6d861650fb
verdict: FAIL / FIX REQUIRED
```

Implementation checkpoint awaiting fresh independent review:

```text
7f652c717c17f40f99a08b126b889a27a84c6376
fix: require complete predicate dependency proof
```

Fresh review request:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-5-request.md
```

Current disposition:

```text
B6.1 PASS  — exact C# invocation semantic identity; keep closed
B6.2 PASS  — conservative configured-item ownership; keep closed
B6.3 PASS  — active module-qualified Angular service ownership; keep closed
B6.4 IMPLEMENTATION GREEN — fresh independent acceptance required
```

B6.4 authority hardening now covers:

1. local/discarded predicates do not become observable rules merely because a LINQ call exists;
2. transformed/polarity-changing return contexts for `Any`, `All`, `First*`, `Single*` fail closed unless modeled;
3. arbitrary `Select` is not an unconditional preserving operation after `Where`;
4. direct identity `Select(card => card)` is proven by symbol identity;
5. same-type method-group projection only preserves authority when the item type is closed/sealed, the projector is a direct object creation, every supported predicate member is directly copied, and every semantic use of the predicate source parameter is itself a supported direct stored-member read;
6. whole-item/unmodeled parameter uses such as `helper(card)`, `card.SomeMethod()`, reference identity, or custom/operator semantics cause conservative downgrade.

Independent re-review 4 counterexample now covered by regression:

```csharp
_cards
    .Where(card => card.IsPublished && IsAllowed(card))
    .Select(CloneCard)
    .ToArray();
```

where `IsAllowed` depends on `Blocked` and `CloneCard` changes `Blocked`.

Focused regression:

```text
Whole_item_helper_dependency_downgrades_same_type_projection_authority
```

The authority contract is now:

```text
exact predicate identity proven
+ context/ownership proven
+ every semantic source-parameter dependency supported and proven
+ every outer operation proven to preserve those semantics
→ authoritative Product Owner rule

otherwise
→ local/lower-authority evidence or omitted product-level claim
```

Exact gates for production checkpoint `7f652c717c17f40f99a08b126b889a27a84c6376`:

```text
CI + PKC tests + WorkPlay + PokeTrade   34933533049 — PASS
pinned Loren                            34933533044 — PASS
Loren-main canary                       34933533104 — PASS
pinned Jellyfin                         34933533050 — PASS
portable parity / no source leak        PASS
```

Core evidence:

```text
PKC build:       0 warnings / 0 errors
C# tests:        76 / 76 PASS
frontend tests:  13 / 13 PASS
WorkPlay:        PASS
PokeTrade:       PASS
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
portable bundle parity:  PASS
portable ZIP parity:     PASS
raw .pkc leak:           none
src/ source-tree leak:   none
artifact id:             10382099834
artifact digest:         sha256:f46bf8b748bb1e044b61c74f73139b23a12d1b2b0e37fcaf17d520dd89a43113
```

V0.4.6 is not complete until a fresh independent reviewer accepts this exact production checkpoint.

If independent re-review returns PASS:

```text
mark V0.4.6 COMPLETE
unlock V0.4.7 as next/current milestone
keep V0.5 Azure DevOps locked
```

Until then, stay in V0.4.6 and do not modify production without a new concrete contradiction.

### V0.4.7 — Cross-layer PO question readiness — LOCKED UNTIL V0.4.6 PASSES

Purpose: broaden the V0.4.6 proof from supported direct patterns into robust cross-layer product-behavior understanding.

Target coverage includes:

- frontend visibility/filter predicates that independently hide or include an item;
- DTO/projection/computed transformations that change observable eligibility/display state;
- richer stores/RxJS/state data-flow where deterministic proof is possible;
- composition of backend predicate + API/DTO transformation + frontend predicate into observable outcome;
- explicit boundaries around DB/remote configuration/feature flags/external state;
- high-signal feature-level promotion so PO-relevant rules are not buried.

Do not start V0.4.7 while V0.4.6 lacks independent PASS.

## V0.5 — Azure DevOps input evidence — LOCKED

Azure DevOps is planned as an additional compiler input describing requirement intent and product/work-item context around code: Epic/Feature/PBI, acceptance intent, sprint/history/status and links through PRs/commits where possible.

ADO must not compensate for missing code-derived business logic.

V0.5 may start only after the V0.4.x PO-question-readiness exit gate independently passes.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
