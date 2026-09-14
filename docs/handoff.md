# PKC Handoff

Use this file when continuing PKC in another coding/review thread.

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

The V0.4.x exit standard is business-logic and PO-question readiness, not merely endpoint/workflow enumeration.

## Current state

```text
V0.4.4  Loren knowledge readiness              PASS / COMPLETE
V0.4.5  Jellyfin generalization               PASS / COMPLETE
V0.4.6  business logic reconstruction         IN PROGRESS / B6.4 OPEN / RE-REVIEW PENDING
V0.4.7  cross-layer PO-question readiness     LOCKED
V0.5    Azure DevOps input evidence           LOCKED
```

Latest independent re-review:

```text
docs/reviews/2026-09-14-v0.4.6-independent-rereview.md
```

That review originally reported:

```text
B6.1  BLOCK
B6.2  PASS
B6.3  BLOCK
B6.4  BLOCK
```

Current coding state is now:

```text
B6.1  IMPLEMENTED + GREEN / independent re-review pending
B6.2  PASS / keep closed
B6.3  IMPLEMENTED + GREEN / independent re-review pending
B6.4  OPEN / next coding task
```

Current implementation HEAD before the docs-only handoff commits:

```text
84da527e8341918b1803e36ccd177a4d14056cd8
```

Never present this previously reviewed/failed implementation as ready again:

```text
45b2d9b6e215f26c395843c77f1e59887774e21b
```

## Read first in the next coding thread

```text
1. docs/status.md
2. docs/handoff.md
3. docs/reviews/2026-09-14-v0.4.6-independent-rereview.md
4. docs/milestones.md
```

Then inspect the current remote `main` before changing anything.

## Accepted baseline

V0.4.4 and V0.4.5 are independently accepted PASS.

V0.4.5 benchmark remains:

```text
jellyfin/jellyfin
1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

Do not reopen accepted gates without concrete contradictory evidence or a new regression caused by later compiler changes.

## B6.1 — implemented and green, not independently re-accepted yet

Independent re-review found that business-predicate semantic authority was only line-level. A custom operation and real LINQ operation on the same line could share proof.

Regression added:

```text
Same_line_custom_Where_cannot_borrow_real_Linq_Where_semantic_authority
```

Regression-first evidence:

```text
RED commit   cf7645eef7b22d82803ea9f48e7575677b8d2a07
RED run      34813594610 — FAIL expected
             C# 1 failed / 59 passed

FIX commit   d784013b54acbec22dc9c18a3cfa738b38adc3d3
GREEN run    34813887033 — PASS
             build 0 warnings / 0 errors
             C# 60 / 60 PASS
             frontend 11 / 11 PASS
             tool pack PASS
             WorkPlay PASS
             PokeTrade PASS
```

Implemented behavior:

```text
semantic invocation relation source carries exact start column
business predicate semantic authority matches exact path + line + column
same-line semantic invocation relations remain distinct during deduplication
```

Do not call B6.1 independently PASS until a new independent review accepts it.

## B6.2 — accepted PASS

Direct configured-item extraction is sufficiently conservative for the reviewed supported forms.

Nested property object initializers are not promoted as additional configured items of the source collection.

Do not modify B6.2 unless a new concrete regression proves a contradiction.

## B6.3 — implemented and green, not independently re-accepted yet

Independent re-review found that Angular cross-stack result/list correlation compared only simple service class names, so two different modules exporting the same service class could cross-link.

Regression added:

```text
Same_service_class_name_in_different_modules_does_not_cross_link_list_flow
```

Regression-first evidence:

```text
RED commit   df1ea834b970201b511c8ff9a975f4e1fe59c53d
RED run      34814066361 — FAIL expected
             frontend 1 failed / 11 passed
             C# 60 / 60 PASS
             observed failure: admin endpoint inherited CatalogComponent list flow

FIX commit   84da527e8341918b1803e36ccd177a4d14056cd8
GREEN CI     34814396514 — PASS
             build 0 warnings / 0 errors
             C# 60 / 60 PASS
             frontend 12 / 12 PASS
             tool pack PASS
             WorkPlay PASS
             PokeTrade PASS
```

Implemented behavior:

```text
API calls carry deterministic module-qualified owner identity
component result bindings resolve injected service identity through named imports
identity format distinguishes module path + exported class
named import aliases normalize to exported class identity
cross-stack result/list linking requires deterministic service identity equality
if deterministic identity is not available, omit that authoritative correlation
```

Cross-benchmark verification for implementation HEAD `84da527e8341918b1803e36ccd177a4d14056cd8`:

```text
pinned Loren       34814396442 — PASS
Loren-main canary  34814396486 — PASS
pinned Jellyfin    34814396458 — PASS
CI + PokeTrade     34814396514 — PASS
```

Jellyfin artifact:

```text
artifact id      10335878381
artifact digest  sha256:1db9c75ee9e404a52a4c8d6e78b11432110ec591412b1b3ca9148f44f7d637c7
```

Do not call B6.3 independently PASS until a new independent review accepts it.

## B6.4 — open blocker and exact next coding task

Current code can prove that a LINQ predicate exists, but then may render authoritative product wording without proving the query result affects the endpoint's observable outcome.

Canonical counterexample:

```csharp
public IReadOnlyList<Card> GetCards()
{
    var published = _cards.Where(card => card.IsPublished).ToArray();
    Audit(published.Length);
    return _cards;
}
```

The LINQ query exists and is reachable, but the endpoint returns unfiltered `_cards`.

PKC must not promote that into a Product Owner claim like:

```text
Includes items from `_cards` only when `card.IsPublished`.
```

Required semantic distinction:

```text
observed query predicate
!=
proven observable business rule
```

Authoritative product wording may only be promoted when deterministic evidence proves the predicate participates in an observable result, for example:

```text
endpoint return value
selection that feeds return
control-flow guard / rejection
mutation or state transition
other externally observable outcome
```

Also preserve context and polarity for operations such as `Any`/`All`.

Example:

```csharp
if (items.Any(x => x.Blocked))
    throw ...;
```

must not be rendered as a positive requirement that blocked items exist.

## Required B6.4 workflow

Do this regression-first:

```text
1. RED
   Add a focused regression where a genuine LINQ predicate is computed and used only for a local/non-observable side path, while the endpoint returns the unfiltered source.

2. Verify red
   Confirm current production code generates an incorrect authoritative rule or otherwise violates the expected distinction.

3. Generic fix
   Keep the query/predicate as observed evidence.
   Add deterministic data/control-flow authority before promoting PO/business-rule wording.
   Do not hardcode fixture names, routes, repository names or operation-specific exceptions.

4. GREEN
   Focused regression must pass.

5. Full gates
   full PKC tests
   PokeTrade known-answer/live acceptance
   pinned Loren
   Loren-main canary
   pinned Jellyfin
   portable parity/no-leak

6. Docs + review
   update docs/status.md and docs/handoff.md
   request a new independent V0.4.6 re-review covering B6.1, B6.3 and B6.4
```

## Scope locks

Do not start V0.4.7.
Do not start Azure DevOps ingestion.

V0.5 remains a future input-evidence source for Epic/Feature/PBI, acceptance intent, history/status and PR/commit linkage. It must not compensate for missing code-derived business logic.

## Copy/paste bootstrap for the next thread

```text
Continue PKC from current remote main HEAD.

Read in order:
1. docs/status.md
2. docs/handoff.md
3. docs/reviews/2026-09-14-v0.4.6-independent-rereview.md
4. docs/milestones.md

Stay in V0.4.6.

B6.1 has already been fixed regression-first and is green; do not redo it unless new evidence contradicts it.
B6.2 is accepted PASS; keep it closed.
B6.3 has already been fixed regression-first and is green; do not redo it unless new evidence contradicts it.

Implement only remaining blocker B6.4 regression-first:
a real/reachable LINQ predicate must remain observed evidence unless deterministic data/control-flow proves that it affects the endpoint's observable return, guard, mutation or other product outcome. Preserve Any/All context and polarity.

After B6.4 green, rerun full PKC tests + PokeTrade + pinned Loren + Loren-main + pinned Jellyfin + portable integrity, update docs, and request independent V0.4.6 re-review for B6.1/B6.3/B6.4.

Do not start V0.4.7 or Azure DevOps.
```
