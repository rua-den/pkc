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
docs/reviews/2026-09-15-v0.4.6-independent-rereview-5.md
reviewed production: 7f652c717c17f40f99a08b126b889a27a84c6376
verdict: FAIL / FIX REQUIRED
```

Current disposition:

```text
B6.1 PASS  — exact C# invocation semantic identity; keep closed
B6.2 PASS  — conservative configured-item ownership; keep closed
B6.3 PASS  — active module-qualified Angular service ownership; keep closed
B6.4 BLOCK — projector can invalidate predicate state after direct-copy proof
```

The re-review-4 dependency-completeness gap is closed: source-parameter uses not proven as direct field/property reads now fail closed.

The remaining B6.4 blocker is narrower. Same-type method-group projection currently proves that each predicate-relevant member has a direct input→output copy, but does not prove that other object-initializer writes cannot mutate that copied state through custom setters or other unproven output effects.

Counterexample class:

```csharp
public sealed class Card
{
    public bool IsPublished { get; set; }

    private bool _blocked;
    public bool Blocked
    {
        get => _blocked;
        set
        {
            _blocked = value;
            IsPublished = false;
        }
    }
}
```

Projector:

```csharp
private static Card CloneCard(Card card) => new()
{
    IsPublished = card.IsPublished,
    Blocked = card.Blocked
};
```

A source item can pass `Where(card => card.IsPublished)`, yet the returned clone can end with `IsPublished == false` after the later `Blocked` setter runs. Current authority proof can still treat the direct `IsPublished` assignment as sufficient.

Required boundary:

```text
exact predicate identity
+ complete predicate dependencies
+ required state copied
+ entire supported projector proven not to invalidate that state
→ authoritative Product Owner rule

otherwise
→ observed-only / omitted authoritative rule
```

Conservative rejection of custom setter / nested / unproven initializer effects is acceptable for V0.4.6.

Exact gates for reviewed production checkpoint `7f652c717c17f40f99a08b126b889a27a84c6376` remain green:

```text
CI + PKC tests + WorkPlay + PokeTrade   34933533049 — PASS
pinned Loren                            34933533044 — PASS
Loren-main canary                       34933533104 — PASS
pinned Jellyfin                         34933533050 — PASS
```

Independent artifact cross-check during re-review 5:

```text
canonical Markdown:             504 files
facts:                          43,363
project-semantic facts:         43,363 / 43,363
bundle canonical parity:        PASS
portable ZIP file-set parity:   PASS
portable ZIP byte parity:       PASS
raw .pkc leak:                  none
src/ source-tree leak:          none
artifact id:                    10382099834
artifact digest:                sha256:f46bf8b748bb1e044b61c74f73139b23a12d1b2b0e37fcaf17d520dd89a43113
```

V0.4.6 remains open until a new implementation checkpoint fixes B6.4 and independently passes review.

### V0.4.7 — Cross-layer PO question readiness — LOCKED UNTIL V0.4.6 PASSES

Purpose: broaden supported direct business-logic patterns into robust cross-layer product-behavior understanding.

Do not start V0.4.7 while V0.4.6 lacks independent PASS.

## V0.5 — Azure DevOps input evidence — LOCKED

Azure DevOps is planned as an additional compiler input for requirement intent, Epic/Feature/PBI history, status and traceability. ADO must not compensate for missing code-derived business logic.

V0.5 may start only after the V0.4.x PO-question-readiness exit gate independently passes.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
