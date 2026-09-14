# PKC Status

Last updated: 2026-09-14

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             FIXES COMPLETE / INDEPENDENT RE-REVIEW REQUIRED
V0.4.7 cross-layer PO-question readiness         LOCKED
V0.5 Azure DevOps input evidence                 LOCKED
```

Previous independent review:

```text
docs/reviews/2026-09-14-v0.4.5-v0.4.6-independent-review.md
```

V0.4.6 re-review request:

```text
docs/reviews/2026-09-14-v0.4.6-independent-rereview-request.md
```

V0.4.6 implementation checkpoint:

```text
478e92343154083c3987f07e4fbad66a042c25e8
```

V0.4.3 remains the last accepted tool package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

No new package version is accepted merely because the V0.4.6 implementation is ready for re-review.

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

Records:

```text
docs/trials/2026-09-14-v0.4.5-jellyfin.md
docs/trials/2026-09-14-v0.4.5-jellyfin-crosscheck.md
```

Independent review accepted V0.4.5. Its warnings W1/W2/W3/W4/W5 remain quality follow-ups but are not reopened by this V0.4.6 work unless a concrete regression appears.

## V0.4.6 — blocker fixes complete / independent re-review required

The previous independent review found exactly three blocker-class false-claim paths. All three now have focused red reproduction, generic fixes and green regression proof.

### B6.1 — semantic authority for LINQ/business-predicate operations

Red regression:

```text
commit: f09658a8f5f097abd4003b15e5261a5388691488
run:    34806787160 — FAIL as expected
C#:     1 failed / 57 passed / 58 total
```

The fixture used a custom `Where(Func<...>)` that ignores its predicate. Before the fix, PKC incorrectly promoted the call to authoritative inclusion logic.

Generic fix:

```text
b13b9d7b0a89f2df0100fa5f8e22a7969b0fdf30
0bc0be87e94472d6a972899a3928085ba3ef636d  positive semantic-proof fixture hardening
```

Authoritative `business-predicate` evidence now requires the project-semantic invocation target at the same invocation source span to resolve exactly to a supported method on:

```text
System.Linq.Enumerable
System.Linq.Queryable
```

The shared rule applies to supported `Where`, `Any`, `All`, `First*` and `Single*` operations. Custom/unresolved same-named methods are conservatively omitted.

Green proof:

```text
run 34806900948 — PASS
C#:       58 / 58
frontend: 10 / 10
PokeTrade PASS
```

### B6.2 — direct configured-item ownership

Red regression:

```text
commit: bec7ff74d48dc7b56b27850f3060d690e1bb311a
run:    34807054449 — FAIL as expected
C#:     1 failed / 58 passed / 59 total
```

The fixture placed a nested `CardMetadata` object inside a configured `Card`. Before the fix, both objects were incorrectly emitted as configured items of `_cards`.

Generic fix:

```text
edb16980d88cd24b6d4052a477f9fb736fc981ad
```

Configured-object extraction now promotes only direct item/value expressions of supported collection/array initializer forms. Nested property object initializers are not additional source items.

Ownership evidence now records:

```text
sourceKind          = direct-collection-item-initializer
ownershipResolution = direct-syntax-parent
```

Focused green proof:

```text
run 34807153439 — PKC test step PASS
```

The final implementation gate below provides the full-suite proof after all three fixes are present together.

### B6.3 — service-aware Angular result/list correlation

Red regression:

```text
commit: aee74969d26627ba7e446de3e28bed5ce6276110
run:    34807296605 — FAIL as expected
frontend: 1 failed / 10 passed / 11 total
C#:       59 / 59 PASS
```

The adversarial fixture contained both `CatalogApi.getCards()` and `AdminApi.getCards()`, while `CatalogComponent` injected only `CatalogApi`. Before the fix, `/api/admin/cards` inherited the Catalog result/list flow solely because the method names matched.

Generic fix chain:

```text
b63d79ba51ab08b82e3e1a468e96974289660d8e  binding service identity
5264f1c6ba8011c901e00360a612be3a0aecc68d  fallback API owner identity
06af278dc4cf51dd3152eaf8693b17e2fa6853d6  AST API owner identity
478e92343154083c3987f07e4fbad66a042c25e8  exact service-owner correlation
```

Cross-stack result/list promotion now requires both method identity and exact service ownership:

```text
binding.apiMethod == apiCall.Container
AND binding.serviceType == apiCall.ownerClass
```

If service ownership cannot be proved, endpoint-to-list correlation is omitted rather than guessed.

Final green proof:

```text
run 34807482174 — PASS
build:          0 warnings / 0 errors
C# tests:       59 / 59
frontend tests: 11 / 11
PokeTrade:      PASS
```

## Exact implementation-checkpoint gates

For implementation checkpoint `478e92343154083c3987f07e4fbad66a042c25e8`:

```text
CI / full PKC / PokeTrade    34807482174 — PASS
pinned Loren                 34807482181 — PASS
Loren-main canary            34807482197 — PASS
pinned Jellyfin              34807482203 — PASS
```

CI/PokeTrade includes:

```text
PKC build                       PASS, 0 warnings / 0 errors
C# tests                        59 / 59 PASS
frontend tests                  11 / 11 PASS
tool pack/install               PASS
WorkPlay knowledge build        PASS
PokeTrade .NET build            PASS
PokeTrade Angular build         PASS
PokeTrade live branch acceptance PASS
PokeTrade knowledge assertions  PASS
```

Jellyfin exact-checkpoint verification:

```text
pinned source normal build      PASS
workflow candidates             386
product features                116
canonical Markdown files        504
facts                           43,700
analysis mode                   project-semantic 43,700 / 43,700
bundle verbatim parity          PASS
portable ZIP exact file set     PASS
portable ZIP byte parity        PASS
raw .pkc leak                   none
src/ source leak                none
artifact id                     10333522300
artifact sha256                 a96bc785c268da30ef03f4282eb64246c964c49599b7443d4020bd80fc049d85
```

## Independent V0.4.6 re-review

The implementation is **not self-declared PASS**. Independent review must challenge the adversarial cases and genericity described in:

```text
docs/reviews/2026-09-14-v0.4.6-independent-rereview-request.md
```

Required independent outcome before any next milestone unlock:

```text
B6.1 no unresolved blocker
B6.2 no unresolved blocker
B6.3 no unresolved blocker
cross-benchmark regression PASS
portable integrity PASS
no repository-specific production exception
```

## Exact next action

```text
independent V0.4.6 re-review only
```

Do not start V0.4.7.
Do not start Azure DevOps ingestion.

## Azure DevOps scope lock

V0.5 remains an additional input-evidence source for Epic/Feature/PBI, acceptance intent, history/status and PR/commit linkage. It must not compensate for missing code-derived business logic.

V0.5 stays locked until the V0.4.x PO-question readiness gates explicitly permit it.
