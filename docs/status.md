# PKC Status

Last updated: 2026-09-14

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             INDEPENDENT RE-REVIEW FAIL / 3 BLOCKERS
V0.4.7 cross-layer PO-question readiness         LOCKED
V0.5 Azure DevOps input evidence                 LOCKED
```

Latest independent V0.4.6 re-review:

```text
docs/reviews/2026-09-14-v0.4.6-independent-rereview.md
```

Reviewed code checkpoint:

```text
478e92343154083c3987f07e4fbad66a042c25e8
```

Reviewed repository HEAD before review-documentation commits:

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

## V0.4.6 — independent re-review FAIL

The prior coding pass genuinely reproduced and fixed the exact original B6.1/B6.2/B6.3 counterexamples regression-first. Independent re-review accepts B6.2 but found remaining authority gaps in B6.1 and B6.3 plus a new B6.4 product-claim gap.

### B6.1 — BLOCK: semantic operation proof is only line-level

Current code requires `System.Linq.Enumerable.<operation>` or `System.Linq.Queryable.<operation>`, which is materially better than lexical naming.

However the proof joins semantic invocation evidence by only:

```text
owner + source path + StartLine + EndLine
```

`SourceLocation` has no exact invocation span/column. A custom `Where(...)` and a real LINQ `Where(...)` on the same source line can therefore share the LINQ proof and promote the custom call to a high-confidence business predicate.

Required regression: custom same-named operation + genuine LINQ operation on the same line; only the genuine invocation may be authoritative.

Required property: exact per-invocation semantic identity, not line-range coincidence.

### B6.2 — PASS: direct configured-item ownership

Nested property object initializers are no longer emitted as additional configured items of the source collection for the reviewed supported forms.

Direct configured items remain available with:

```text
sourceKind          = direct-collection-item-initializer
ownershipResolution = direct-syntax-parent
```

Do not reopen B6.2 without a new concrete contradiction.

### B6.3 — BLOCK: Angular service identity is only a simple class name

Current result/list correlation requires method name + service type == API owner class. This closes the original `CatalogApi` versus `AdminApi` case.

But two different modules can both export `CardsApi`. A component importing one `CardsApi` and an unrelated endpoint owned by the other `CardsApi` still compare equal by simple class name and can cross-link list flow.

Required regression: two modules exporting the same class + method name with different routes. Only the actually imported/injected module may feed the component list.

Required property: module-qualified/deterministic service identity where provable; otherwise omit the correlation.

### B6.4 — BLOCK: proven LINQ operation does not prove observable endpoint behavior

Current code promotes every supported LINQ predicate attached to a reachable callable as an endpoint business rule, with wording such as:

```text
Includes items from `_cards` only when `...`.
Requires at least one item from `_cards` to satisfy `...`.
```

It does not yet prove that the LINQ result determines the endpoint return, guard, mutation or another observable outcome.

Counterexample:

```csharp
var published = _cards.Where(card => card.IsPublished).ToArray();
Audit(published.Length);
return _cards;
```

The LINQ call is real, but the endpoint returns unfiltered cards. Rendering it as endpoint inclusion logic is a false Product Owner claim.

Required property: separate observed local query predicates from proven observable business rules. Promote authoritative product wording only when deterministic control/data flow proves the predicate participates in the observable outcome.

## Regression-first evidence already verified

Original red runs were genuine:

```text
B6.1 red  f09658a8f5f097abd4003b15e5261a5388691488 / run 34806787160 — FAIL
B6.2 red  bec7ff74d48dc7b56b27850f3060d690e1bb311a / run 34807054449 — FAIL
B6.3 red  aee74969d26627ba7e446de3e28bed5ce6276110 / run 34807296605 — FAIL
```

## Exact reviewed-head gates

For reviewed HEAD `45b2d9b6e215f26c395843c77f1e59887774e21b`:

```text
CI / PokeTrade        34807855630 — PASS
pinned Loren          34807855596 — PASS
Loren-main push       34807855650 — PASS
Loren-main scheduled  34807965935 — PASS
pinned Jellyfin       34807855635 — PASS
```

Current-head CI:

```text
build           0 warnings / 0 errors
C# tests        59 / 59 PASS
frontend tests  11 / 11 PASS
tool pack       PASS
WorkPlay        PASS
PokeTrade       PASS
```

Current-head Jellyfin artifact:

```text
artifact id:     10334150660
artifact digest: sha256:17b546f3119f4350dfffa5fc22833a80872a76c677005ca0c2c66810c6f79f82
```

Independent portable recheck:

```text
canonical Markdown                  504
bundle canonical content            504 / 504
portable ZIP files                  504
ZIP set parity                      PASS
ZIP byte parity                     504 / 504
raw/source leak                     0
PKC_KNOWLEDGE.md sha256             737535fc3bc9fc04d07a2cc8a53d8bfd2a427b297dedba3cc38986f8e84ae55f
PKC_KNOWLEDGE.zip sha256            a2c2f876ae76f7a24a66b7219bbc970cf21545b4bec315e7919d64cda38be809
```

Green automation and portable integrity do not override the semantic false-claim blockers.

## Exact next action

Stay in V0.4.6. Fix only:

```text
B6.1 exact invocation identity for semantic operation proof
B6.3 module-qualified/deterministic Angular service identity
B6.4 observable-effect proof before product-rule promotion
```

Each fix must be regression-first:

```text
focused red
→ verify failure
→ generic fix
→ focused green
```

Then rerun full PKC tests, PokeTrade, pinned Loren, Loren-main, pinned Jellyfin and portable parity/no-leak, update status/handoff and request independent V0.4.6 re-review.

Do not start V0.4.7.
Do not start Azure DevOps ingestion.
