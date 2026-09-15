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

PKC must preserve three distinct knowledge classes as it improves product-level synthesis: authoritative business conditions, lower-authority value lineage/provenance, and mutation/causality evidence. Conservative downgrade must prevent false business claims without deleting deterministic causal evidence that can answer `where did this value come from?` or `why did this value/status change?` questions. See `docs/product-knowledge-contract.md`.

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

### V0.4.6 — Business logic reconstruction — CURRENT / IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING

Purpose: compile deterministic business-decision evidence strongly enough that an AI can answer practical `when`, `why`, `which conditions` and `what makes this visible/eligible` questions from generated knowledge.

Latest completed independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-5.md
reviewed production: 7f652c717c17f40f99a08b126b889a27a84c6376
verdict: FAIL / FIX REQUIRED
```

Implementation checkpoint awaiting fresh independent review:

```text
18a1f1d1ef551833d859f23ea2e92dd548a6a81d
fix: fail closed on unsafe same-type projector effects
```

Fresh review request:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-6-request.md
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
5. same-type method-group projection requires a closed/sealed item type, complete predicate dependencies and direct object creation;
6. unsupported whole-item/unmodeled predicate uses such as `helper(card)`, `card.SomeMethod()`, reference identity or custom/operator semantics cause conservative downgrade;
7. every supported clone initializer entry must now be a simple assignment to a direct stored non-static field or auto-property;
8. every initializer assignment must be an exact same-member input→output copy, so custom setters, nested/unmodeled effects, helper/constant rewrites and other unproven projector mutations cannot preserve product-level `Where` authority.

Focused regression for re-review-5 blocker:

```text
Later_custom_setter_that_invalidates_predicate_state_downgrades_projection_authority
```

The regression uses behavior-valid source initializer ordering (`Blocked = false, IsPublished = true`) so the source reaches `Where` with `IsPublished == true`; the returned clone then demonstrates the custom-setter invalidation path. The independent re-review-5 record is intentionally not rewritten.

The authority contract is now:

```text
exact predicate identity proven
+ context/ownership proven
+ every semantic source-parameter dependency supported and proven
+ every outer operation proven to preserve semantics
+ every supported projector write proven side-effect-safe within the closed model
→ authoritative Product Owner rule

otherwise
→ local/lower-authority evidence or omitted product-level claim
```

This is deliberately an authority boundary, not a rule to discard deterministic causality. Lower-authority value-lineage/mutation evidence remains a first-class product direction under `docs/product-knowledge-contract.md`.

Exact gates for production checkpoint `18a1f1d1ef551833d859f23ea2e92dd548a6a81d`:

```text
CI + PKC tests + WorkPlay + PokeTrade   34954590262 — PASS
pinned Loren                            34954590188 — PASS
Loren-main canary                       34954590239 — PASS
pinned Jellyfin                         34954590155 — PASS
portable parity / no source leak        PASS
```

Core evidence:

```text
PKC build:       0 warnings / 0 errors
C# tests:        77 / 77 PASS
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
artifact id:             10390258392
artifact digest:         sha256:6ba875d99cf64141267bb13811485cc025eabc87723beaa9f3a46a985860b497
artifact size:           9,016,989 bytes
```

V0.4.6 is not complete until a fresh independent reviewer accepts exact production checkpoint `18a1f1d1ef551833d859f23ea2e92dd548a6a81d`.

### V0.4.7 — Cross-layer PO question readiness — LOCKED UNTIL V0.4.6 PASSES

Purpose: broaden supported direct business-logic patterns into robust cross-layer product-behavior understanding.

Planned target coverage includes, without starting implementation before V0.4.6 passes:

- frontend visibility/filter predicates that independently affect observable outcomes;
- DTO/projection/computed transformations that change observable state;
- cross-entity value lineage such as `ProductGroup.A → Product.A → Service.A` when source proves copy/derivation steps;
- snapshot/copy versus dynamic/reference-derived value semantics;
- later overrides and mutation/causality paths that explain why a persisted or returned value changed;
- preservation of lower-authority causal evidence even when product-rule authority is downgraded;
- composition of backend conditions, data/value flow and frontend behavior into a PO-facing explanation;
- high-signal promotion so useful causal/value-origin knowledge is not buried in raw implementation noise.

Do not start V0.4.7 while V0.4.6 lacks independent PASS.

## V0.5 — Azure DevOps input evidence — LOCKED

Azure DevOps is planned as an additional compiler input for requirement intent, Epic/Feature/PBI history, status and traceability. ADO must not compensate for missing code-derived business logic.

Until ADO is integrated, its absence should be declared as a global knowledge boundary rather than repeated as an `unknown` placeholder in every feature/workflow Markdown file. Feature-local unknowns remain appropriate only when the missing value materially affects that feature's answer.

V0.5 may start only after the V0.4.x PO-question-readiness exit gate independently passes.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
