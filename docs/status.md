# PKC Status

Last updated: 2026-09-14

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             IN PROGRESS / B6.4 OPEN / RE-REVIEW PENDING
V0.4.7 cross-layer PO-question readiness         LOCKED
V0.5 Azure DevOps input evidence                 LOCKED
```

Latest independent V0.4.6 re-review:

```text
docs/reviews/2026-09-14-v0.4.6-independent-rereview.md
```

That re-review disposition was:

```text
B6.1  BLOCK
B6.2  PASS
B6.3  BLOCK
B6.4  BLOCK
```

Current coding checkpoint has since implemented regression-first fixes for B6.1 and B6.3. They are green across current gates but are **not independently re-accepted yet**. B6.4 remains the only coding blocker still open.

Current implementation HEAD before this status/handoff checkpoint:

```text
84da527e8341918b1803e36ccd177a4d14056cd8
```

Previously failed reviewed implementation — do not present this as ready again:

```text
45b2d9b6e215f26c395843c77f1e59887774e21b
```

V0.4.3 remains the last accepted tool package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

## Product contract governing V0.4.x

PKC must generate portable knowledge rich enough for an AI to answer practical Product Owner questions about observable behavior, business conditions and cross-layer outcomes without re-reading source code.

Representative question:

> When is entity X sellable/visible on the web, and what exact conditions must be true for it to appear?

Required evidence chain where source can prove it:

```text
configured/static values
→ business predicates and boolean semantics
→ backend selection/eligibility
→ API/result flow
→ DTO/computed transformation when relevant
→ frontend visibility/list/filter behavior
→ observable product outcome
```

No deterministic proof means no authoritative product claim. Runtime database/config/external values remain unknown unless grounded by another input.

## V0.4.5 — PASS / COMPLETE

Pinned benchmark:

```text
jellyfin/jellyfin
1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

Independent review accepted V0.4.5. W1/W2/W3/W4/W5 remain non-blocking quality follow-ups unless a concrete regression appears.

## V0.4.6 blocker status

### B6.1 — IMPLEMENTED + GREEN / independent re-review pending

Problem found by re-review: semantic operation authority was associated only by owner + path + line range, so a custom same-named invocation could borrow LINQ authority from a genuine invocation on the same line.

Regression-first sequence:

```text
RED commit   cf7645eef7b22d82803ea9f48e7575677b8d2a07
RED run      34813594610 — FAIL expected
             C# 1 failed / 59 passed
             exact failure: same-line custom Where borrowed genuine LINQ authority

FIX commit   d784013b54acbec22dc9c18a3cfa738b38adc3d3
GREEN run    34813887033 — PASS
             build 0 warnings / 0 errors
             C# 60 / 60 PASS
             frontend 11 / 11 PASS
             tool pack PASS
             WorkPlay PASS
             PokeTrade PASS
```

Implemented property:

```text
semantic invocation relations carry exact start column
business-predicate authority requires exact path + line + column match
relation dedup preserves distinct same-line invocations
```

This closes the coding counterexample but still needs independent re-review before B6.1 is called accepted PASS.

### B6.2 — PASS / keep closed

Nested property object initializers are no longer emitted as additional configured items of the source collection for the reviewed supported forms.

Direct configured items remain available with:

```text
sourceKind          = direct-collection-item-initializer
ownershipResolution = direct-syntax-parent
```

Do not reopen B6.2 without a new concrete contradiction.

### B6.3 — IMPLEMENTED + GREEN / independent re-review pending

Problem found by re-review: Angular cross-stack list/result correlation used only simple service class names, so two different modules exporting the same class and method name could cross-link.

Regression-first sequence:

```text
RED commit   df1ea834b970201b511c8ff9a975f4e1fe59c53d
RED run      34814066361 — FAIL expected
             frontend 1 failed / 11 passed
             C# 60 / 60 PASS
             exact failure: admin endpoint inherited CatalogComponent list flow

FIX commit   84da527e8341918b1803e36ccd177a4d14056cd8
GREEN CI     34814396514 — PASS
             build 0 warnings / 0 errors
             C# 60 / 60 PASS
             frontend 12 / 12 PASS
             tool pack PASS
             WorkPlay PASS
             PokeTrade PASS
```

Implemented property:

```text
API owner identity is module-qualified, not simple-name-only
component result-binding service identity resolves imported module + exported class
cross-stack correlation requires matching deterministic service identity
named import aliases are normalized to the exported class identity
when deterministic module identity cannot be proven, no authoritative service match is claimed
```

Cross-benchmark gates for `84da527e8341918b1803e36ccd177a4d14056cd8`:

```text
pinned Loren       34814396442 — PASS
Loren-main canary  34814396486 — PASS
pinned Jellyfin    34814396458 — PASS
CI + PokeTrade     34814396514 — PASS
```

Current Jellyfin artifact:

```text
artifact id:     10335878381
artifact digest: sha256:1db9c75ee9e404a52a4c8d6e78b11432110ec591412b1b3ca9148f44f7d637c7
```

This closes the coding counterexample but still needs independent re-review before B6.3 is called accepted PASS.

### B6.4 — OPEN BLOCKER / next coding task

Current code can prove that a supported LINQ predicate exists and is reachable, but that alone does not prove the predicate determines an endpoint's observable behavior.

Canonical counterexample:

```csharp
public IReadOnlyList<Card> GetCards()
{
    var published = _cards.Where(card => card.IsPublished).ToArray();
    Audit(published.Length);
    return _cards;
}
```

The query is real, but the endpoint returns unfiltered cards. PKC must not render this as:

```text
Includes items from `_cards` only when `card.IsPublished`.
```

Required property:

```text
observed local query predicate != authoritative observable business rule
```

Authoritative PO wording may only be promoted when deterministic control/data-flow evidence proves the predicate participates in an observable return, guard, mutation, selection or other product outcome.

Also preserve context/polarity for operations such as `Any`/`All`; for example:

```csharp
if (items.Any(x => x.Blocked)) throw ...;
```

must not become a positive requirement that blocked items exist.

## Historical independent review checkpoint

The failed independent re-review evaluated the earlier implementation around:

```text
reviewed implementation checkpoint  478e92343154083c3987f07e4fbad66a042c25e8
reviewed code HEAD                    45b2d9b6e215f26c395843c77f1e59887774e21b
```

Exact gates for that old reviewed HEAD were green, but green automation did not override semantic false-claim blockers. Do not use that HEAD as a new review candidate.

## Exact next action

Stay in V0.4.6 and work **only B6.4** regression-first:

```text
1. Add focused red regression:
   a genuine LINQ predicate is computed/reachable but its result does not influence the endpoint's observable result.

2. Verify expected failure on current code.

3. Implement generic observable-effect authority:
   - retain local query/predicate evidence as observed evidence;
   - promote PO/business-rule wording only when deterministic control/data-flow proves observable participation;
   - preserve guard polarity/context for Any/All and selection operations.

4. Run focused green.

5. Rerun full gates:
   - full PKC tests
   - PokeTrade
   - pinned Loren
   - Loren-main canary
   - pinned Jellyfin
   - portable parity/no-leak

6. Update status/handoff and request independent V0.4.6 re-review covering B6.1, B6.3 and B6.4.
```

Do not start V0.4.7.
Do not start Azure DevOps ingestion.
