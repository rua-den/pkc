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

### V0.4.6 — Business logic reconstruction — COMPLETE / INDEPENDENT REVIEW PASS

Purpose: compile deterministic business-decision evidence strongly enough that an AI can answer practical `when`, `why`, `which conditions` and `what makes this visible/eligible` questions from generated knowledge.

Final independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-8.md
reviewed production: 868195eff5435cca1c98d4bf6ffd4b18018daf66
verdict: PASS / COMPLETE
```

Accepted production checkpoint:

```text
868195eff5435cca1c98d4bf6ffd4b18018daf66
fix: require inert clone construction
```

Final disposition:

```text
B6.1 PASS — exact C# invocation semantic identity; keep closed
B6.2 PASS — conservative configured-item ownership; keep closed
B6.3 PASS — active module-qualified Angular service ownership; keep closed
B6.4 PASS — constructor-effect authority gap closed; keep closed
```

Accepted B6.4 hardening covers:

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

#### Accepted constructor-effect checkpoint

Rereview 7 demonstrated that a copy constructor could mutate the source item after `Where` passed and before initializer values were copied. Production `868195eff...` closes that authority gap with a conservative generic boundary:

```text
complete predicate dependencies
+ same closed item type
+ zero-argument object creation
+ exact compiler-generated implicit constructor on projected type
+ no unproven base-constructor path
+ no instance field/event/property initializer code
+ every initializer write proven safe
+ every required predicate member copied
→ authoritative returned-item predicate may be retained

otherwise
→ observed-only / omitted authoritative rule
```

Regression coverage includes source-mutating copy constructors, user-defined parameterless constructors, implicit construction with instance initializers, and implicit construction with an effectful base constructor. Existing positive implicit inert defensive-clone behavior remains green.

Independent rereview 8 additionally challenged constructor overloads, optional/`params` zero-argument calls, target-typed `new`, explicit `new Type()`, parenthesized creation, partial declarations, inheritance/base constructors and semantic candidate fallback. No compile-valid/behavior-valid bypass was found.

The rereview-6 callback/comparer pipeline boundary remains accepted. No benchmark-specific exception is present. Authority downgrade continues to preserve deterministic lower-authority mutation/provenance evidence.

The reviewer environment lacked `dotnet`, so no local runtime rerun is claimed. Exact-production CI is green:

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

Pinned Jellyfin evidence:

```text
source build:         0 warnings / 0 errors
facts:                43,363
relations:            195,314
workflow candidates:  386
product features:     116
canonical Markdown:   504
project-semantic:     43,363 / 43,363
artifact id:          10394456737
digest:               sha256:586863e971967be62ec6e0a7763cc90806903621d43ea896813e4cf3b3a2e414
size:                 9,016,935 bytes
portable parity:      PASS
portable ZIP parity:  PASS
raw .pkc leak:        NONE
src/ leak:            NONE
```

V0.4.6 is closed by independent rereview 8.

### V0.4.7 — Cross-layer PO question readiness — CURRENT / NEXT MILESTONE

Purpose: broaden supported direct business-logic patterns into robust cross-layer product-behavior understanding.

Planned target coverage includes frontend visibility predicates, DTO/projection transformations, cross-entity value lineage, snapshot/copy versus dynamic semantics, later overrides/mutation causality, and cross-layer PO-facing explanations.

V0.4.7 must preserve every accepted V0.4.6 authority and evidence-retention guardrail. Its concrete acceptance scope and regression gates should be defined before implementation advances.

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
