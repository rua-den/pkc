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

### V0.4.6 — Business logic reconstruction — CURRENT / INDEPENDENT RE-REVIEW FAIL / 1 BLOCKER

Purpose: compile deterministic business-decision evidence strongly enough that an AI can answer practical `when`, `why`, `which conditions` and `what makes this visible/eligible` questions from generated knowledge.

Latest independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-7.md
reviewed production: eb0903ef93b2b85669ded0e2227ca1a950bc49c7
verdict: FAIL / FIX REQUIRED
```

Current disposition:

```text
B6.1 PASS — exact C# invocation semantic identity; keep closed
B6.2 PASS — conservative configured-item ownership; keep closed
B6.3 PASS — active module-qualified Angular service ownership; keep closed
B6.4 BLOCK — same-type projector constructor effects are not proven safe
```

Accepted B6.4 hardening already covers:

1. discarded/local predicates do not become observable rules merely because a LINQ call exists;
2. transformed/polarity-changing return contexts for `Any`, `All`, `First*`, `Single*` fail closed unless modeled;
3. arbitrary `Select` is not an unconditional preserving operation after `Where`;
4. direct identity `Select(card => card)` is proven by symbol identity;
5. unsupported whole-item predicate dependencies cause conservative downgrade;
6. same-type method-group projection requires a closed item type and direct same-member initializer copies;
7. custom setter / nested / rewritten initializer effects fail closed;
8. callback/comparer-bearing pipeline operations are not trusted merely from LINQ method identity;
9. callback-free pipeline preservation is limited to an audited exact-shape subset.

#### Rereview-7 remaining blocker

The same-type method-group defensive-clone path validates object initializer assignments but does not validate object-constructor effects.

Counterexample:

```csharp
public sealed class Card
{
    public bool IsPublished { get; set; }
    public bool Blocked { get; set; }

    public Card() { }
    public Card(Card source) => source.IsPublished = false;
}

public IReadOnlyList<Card> GetCards() =>
    _cards
        .Where(card => card.IsPublished)
        .Select(CloneCard)
        .ToArray();

private static Card CloneCard(Card card) => new Card(card)
{
    IsPublished = card.IsPublished,
    Blocked = card.Blocked
};
```

Execution:

```text
Where passes while IsPublished == true
→ projector constructor mutates source IsPublished = false
→ initializer copies false
→ returned clone has IsPublished == false
```

The current proof can still preserve the earlier `Where` predicate because constructor arguments/body are outside the modeled safe-initializer boundary.

Required generic authority boundary:

```text
complete predicate dependencies
+ same closed item type
+ object-construction path proven effect-safe
+ every initializer write proven safe
+ every required predicate member copied
→ authoritative returned-item predicate may be retained

otherwise
→ observed-only / omitted authoritative rule
```

A conservative V0.4.6 fix may require an implicit/default inert constructor and fail closed for constructor arguments or user-defined constructor code unless deterministically proven safe.

Exact gates for the reviewed production checkpoint remain green:

```text
CI + PKC tests + WorkPlay + PokeTrade   34959028651 — PASS
pinned Loren                            34959028686 — PASS
Loren-main canary                       34959028633 — PASS
pinned Jellyfin                         34959028626 — PASS
```

Core evidence:

```text
PKC build:       0 warnings / 0 errors
C# tests:        81 / 81 PASS
frontend tests:  13 / 13 PASS
WorkPlay:        PASS
PokeTrade:       PASS
```

Pinned Jellyfin artifact:

```text
id:      10391499835
digest:  sha256:39aab5f88914a2e153646b75fa9c8b6cece8a03f3f2dbc821fae871dab820dc2
size:    9,016,935 bytes
```

V0.4.6 remains open until a new implementation checkpoint closes this constructor-effect authority gap and a fresh independent review passes it.

### V0.4.7 — Cross-layer PO question readiness — LOCKED UNTIL V0.4.6 PASSES

Purpose: broaden supported direct business-logic patterns into robust cross-layer product-behavior understanding.

Planned target coverage includes frontend visibility predicates, DTO/projection transformations, cross-entity value lineage, snapshot/copy versus dynamic semantics, later overrides/mutation causality, and cross-layer PO-facing explanations.

Do not start V0.4.7 while V0.4.6 lacks independent PASS.

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
