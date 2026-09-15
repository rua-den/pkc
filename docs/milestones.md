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

The exit standard is not merely endpoint/workflow coverage. PKC must preserve enough grounded logic to answer questions of this class:

> When is entity X sellable/visible on the web, and what exact conditions must be true for it to appear?

That requires deterministic evidence for relevant predicates, configured/static values, data flow and frontend presentation where source can prove them, plus explicit unknowns where runtime state cannot be proven statically.

### V0.4.1 — Frontend adapter architecture — COMPLETE

- `IFrontendAdapter` contract
- framework-agnostic `FrontendScanner`
- React and Angular adapters emit the same canonical `ui-*` facts
- generic `UI action → API call` relation linker
- CLI has no Angular-vs-React branch

### V0.4.2 — PokeTrade real-system benchmark — COMPLETE

The runnable `.NET 10 + Angular 22` PokeTrade application is the known-answer behavioral acceptance benchmark.

### V0.4.3 — Analyzer fidelity hardening — COMPLETE

Completed semantic/fallback provenance and frontend/backend analyzer fidelity hardening.

Last accepted tool package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

### V0.4.4 — Loren knowledge readiness — COMPLETE / EXTERNAL REVIEW PASS

Purpose: prove that the generated portable pack can explain a genuine product/system at the correct abstraction level without source access.

Final independent review:

```text
docs/reviews/2026-09-14-v0.4.4-external-rereview-4.md
```

Accepted baseline includes PokeTrade, pinned Loren, Loren-main and portable handoff parity, with conservative cross-stack semantic authority.

### V0.4.5 — Independent real-repository generalization gate — COMPLETE / INDEPENDENT REVIEW PASS

Purpose: prove PKC did not simply overfit PokeTrade + Loren before deepening business-logic reconstruction.

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

The blind pass was frozen before source inspection. Source cross-check exposed a generic inherited ASP.NET Core controller-route defect. The issue was reproduced regression-first and fixed generically while preserving explicit derived `[Route("")]` override behavior.

V0.4.5 is accepted with non-blocking warnings:

```text
W3 duplicate HTTP verb extraction
W4 feature-level rule promotion
W5 large-pack signal/noise
```

PokeTrade + Loren + Jellyfin regressions remained green on the reviewed candidate.

### V0.4.6 — Business logic reconstruction — CURRENT / INDEPENDENT RE-REVIEW FAIL / 1 BLOCKER

Purpose: compile deterministic business-decision evidence strongly enough that an AI can answer practical `when`, `why`, `which conditions` and `what makes this visible/eligible` questions from generated knowledge.

Latest independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-4.md
reviewed production: a636172ea8575f46d51e503d1ba7ad6d861650fb
verdict: FAIL / FIX REQUIRED
```

Current disposition:

```text
B6.1 PASS  — exact C# invocation semantic identity; keep closed
B6.2 PASS  — conservative configured-item ownership; keep closed
B6.3 PASS  — active module-qualified Angular service ownership; keep closed
B6.4 BLOCK — same-type projection proof ignores unmodeled whole-item predicate dependencies
```

B6.1 remains closed by exact invocation semantic re-resolution through Roslyn using source path + syntax `SpanStart`.

B6.2 remains closed by direct configured-item ownership; nested property objects are not promoted as collection items.

B6.3 remains independently accepted. Module-qualified service ownership requires lexically active relative import evidence. Comment/string/template import-like text has zero authority; conflicting local-name imports are ambiguous; unresolved ownership is omitted rather than guessed.

B6.4 has progressed through several authority hardening steps:

1. local/discarded predicates no longer become observable rules merely because the LINQ call exists;
2. polarity and transformed return contexts for `Any`, `All`, `First*`, `Single*` are conservative;
3. arbitrary `Select` is no longer an unconditional preserving operation after `Where`;
4. direct identity `Select(card => card)` is proven by symbol identity;
5. a narrow same-type method-group clone may preserve authority only when discovered predicate members are copied directly.

The remaining gap is that the discovered member set is not proven complete.

Counterexample:

```csharp
public IReadOnlyList<Card> GetCards() =>
    _cards
        .Where(card => card.IsPublished && IsAllowed(card))
        .Select(CloneCard)
        .ToArray();

private static bool IsAllowed(Card card) => !card.Blocked;

private static Card CloneCard(Card card) => new()
{
    IsPublished = card.IsPublished,
    Blocked = true
};
```

The current collector sees `card.IsPublished`, but not the whole-item helper dependency `IsAllowed(card)`. It can therefore prove only `IsPublished` copied, allow `Blocked` to change, and still promote the full predicate expression as authoritative.

Possible false knowledge:

```text
Includes items from `_cards` only when `card.IsPublished && IsAllowed(card)`.
```

The returned clone violates `IsAllowed`, so this is a blocker-class false Product Owner claim.

Required authority boundary:

```text
exact predicate identity proven
+ context/ownership proven
+ every semantic source-parameter dependency proven
+ every outer operation proven to preserve those semantics
→ authoritative Product Owner rule

otherwise
→ local/lower-authority evidence or omitted product-level claim
```

For V0.4.6, conservative omission is acceptable. A same-type projection should downgrade if the predicate source parameter has unsupported whole-item uses such as helper calls, instance methods, object/reference identity checks, custom/operator semantics, or other unmodeled dependency paths.

Required focused regression:

```text
Where(card => card.IsPublished && IsAllowed(card))
→ Select(CloneCard)
```

where the helper depends on a member changed by the clone.

Reviewed production checkpoint:

```text
a636172ea8575f46d51e503d1ba7ad6d861650fb
fix: preserve proven same-type Select projections
```

Exact gates for that checkpoint are green but do not override the semantic blocker:

```text
CI + PKC tests + WorkPlay + PokeTrade   34931116584 — PASS
pinned Loren                            34931116570 — PASS
Loren-main canary                       34931116599 — PASS
pinned Jellyfin                         34931116503 — PASS
portable parity / no source leak        PASS
```

Exact core test evidence:

```text
PKC build:       0 warnings / 0 errors
C# tests:        75 / 75 PASS
frontend tests:  13 / 13 PASS
WorkPlay:        PASS
PokeTrade:       PASS
```

Pinned Jellyfin remains:

```text
jellyfin/jellyfin @ 1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
facts:                 43,363
relations:             195,314
workflow candidates:   386
product features:      116
canonical Markdown:    504 files
analysis mode:         project-semantic for 43,363 / 43,363 facts
portable bundle parity: PASS
portable ZIP parity:    PASS
raw .pkc leak:          none
src/ source-tree leak:  none
artifact id:            10381642948
artifact digest:        sha256:1e4df6f7c0b77ce7a72488462c209b4b5a783647f88b1fb7c9c737e8a7ba507f
```

Exact next step:

```text
stay in V0.4.6
→ focused red regression for incomplete predicate-dependency proof
→ minimum generic conservative fix
→ focused/related/full validation
→ one coherent implementation checkpoint/push
→ rerun all current gates
→ fresh independent review
```

Do not mark V0.4.6 complete until independent PASS.

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

V0.5 may start only after the V0.4.x PO-question-readiness exit gate independently passes. At minimum:

```text
PokeTrade known-answer PO regression              PASS
Loren blind knowledge-only comprehension          PASS
Jellyfin independent real-repo generalization     PASS
business-logic reconstruction                     PASS
cross-layer PO-question readiness                 PASS
cross-benchmark regression                        PASS
known boundaries documented honestly              PASS
independent external review                       PASS
```

If any gate is not PASS, remain in V0.4.x.

## V0.6 — Incremental compilation

Use file/symbol hashes and dependency impact to rebuild only affected evidence and knowledge. Add PR/CI knowledge diffs.

## V0.7 — Runtime UI exploration

Use browser automation to confirm actual user-visible flows and states.

## V0.8 — Product insight

Surface gaps, inconsistencies, requirement-vs-implementation drift and improvement opportunities with evidence.
