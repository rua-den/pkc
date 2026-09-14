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
V0.4.6  business logic reconstruction         FIXES COMPLETE / INDEPENDENT RE-REVIEW REQUIRED
V0.4.7  cross-layer PO-question readiness     LOCKED
V0.5    Azure DevOps input evidence           LOCKED
```

Implementation checkpoint:

```text
478e92343154083c3987f07e4fbad66a042c25e8
```

Previous review:

```text
docs/reviews/2026-09-14-v0.4.5-v0.4.6-independent-review.md
```

Re-review request:

```text
docs/reviews/2026-09-14-v0.4.6-independent-rereview-request.md
```

## Read first

```text
1. docs/status.md
2. docs/handoff.md
3. docs/milestones.md
4. docs/reviews/2026-09-14-v0.4.5-v0.4.6-independent-review.md
5. docs/reviews/2026-09-14-v0.4.6-independent-rereview-request.md
6. docs/trials/2026-09-14-v0.4.5-jellyfin.md
7. docs/trials/2026-09-14-v0.4.5-jellyfin-crosscheck.md
```

## Accepted baseline — V0.4.5

V0.4.5 is independently accepted PASS.

Pinned benchmark:

```text
jellyfin/jellyfin
1d7b6d97844c8cc848ed3fb5c4b48bb9cdd5b139
```

Do not reopen V0.4.5 without new contradictory evidence or a concrete regression caused by later compiler changes.

Warnings W1/W2/W3/W4/W5 remain quality follow-ups.

## V0.4.6 blocker closure

The previous independent review found exactly B6.1, B6.2 and B6.3. No other V0.4.6 scope was implemented in this coding pass.

### B6.1 — semantic business-predicate operation proof

Red:

```text
f09658a8f5f097abd4003b15e5261a5388691488
run 34806787160 — FAIL as expected
C# 1 failed / 57 passed / 58 total
```

Counterexample was a user-defined `Where` method that ignored its predicate. Old code trusted lexical name + lambda shape.

Fix:

```text
b13b9d7b0a89f2df0100fa5f8e22a7969b0fdf30
0bc0be87e94472d6a972899a3928085ba3ef636d  positive proof regression
```

Authoritative predicate evidence now requires an exact project-semantic invocation target at the same source span and only promotes supported operations resolved to:

```text
System.Linq.Enumerable.<operation>
System.Linq.Queryable.<operation>
```

The shared operation map covers `Where`, `Any`, `All`, `First*` and `Single*`. Custom/unresolved same-named methods do not become authoritative business rules.

Green:

```text
run 34806900948 — PASS
C# 58 / 58
frontend 10 / 10
PokeTrade PASS
```

### B6.2 — direct configured-item ownership

Red:

```text
bec7ff74d48dc7b56b27850f3060d690e1bb311a
run 34807054449 — FAIL as expected
C# 1 failed / 58 passed / 59 total
```

Counterexample nested `CardMetadata` under a direct configured `Card`; old recursive traversal emitted both as collection items.

Fix:

```text
edb16980d88cd24b6d4052a477f9fb736fc981ad
```

Only direct supported collection/array initializer item/value expressions are emitted as configured items. Nested property object initializers are not additional items.

Ownership metadata:

```text
sourceKind          = direct-collection-item-initializer
ownershipResolution = direct-syntax-parent
```

Focused green:

```text
run 34807153439 — PKC test step PASS
```

### B6.3 — Angular service-aware result/list correlation

Red:

```text
aee74969d26627ba7e446de3e28bed5ce6276110
run 34807296605 — FAIL as expected
frontend 1 failed / 10 passed / 11 total
C# 59 / 59 PASS
```

Counterexample contained both:

```text
CatalogApi.getCards() → /api/cards
AdminApi.getCards()   → /api/admin/cards
```

while `CatalogComponent` injected only `CatalogApi`. Old code linked the Catalog list to the admin endpoint because the method names matched.

Fix chain:

```text
b63d79ba51ab08b82e3e1a468e96974289660d8e  binding service type
5264f1c6ba8011c901e00360a612be3a0aecc68d  fallback API owner class
06af278dc4cf51dd3152eaf8693b17e2fa6853d6  AST API owner class
478e92343154083c3987f07e4fbad66a042c25e8  require exact ownership
```

Authoritative cross-stack result/list linkage now requires:

```text
binding.apiMethod == apiCall.Container
AND binding.serviceType == apiCall.ownerClass
```

Supported service-type proof includes Angular `inject(ServiceType)`, constructor DI and direct `new ServiceType(...)`. If ownership is not proven, PKC omits the cross-stack list claim.

Final green:

```text
run 34807482174 — PASS
C# 59 / 59
frontend 11 / 11
PokeTrade PASS
```

## Exact implementation-checkpoint gates

For `478e92343154083c3987f07e4fbad66a042c25e8`:

```text
CI + PokeTrade      34807482174 — PASS
pinned Loren        34807482181 — PASS
Loren-main canary   34807482197 — PASS
pinned Jellyfin     34807482203 — PASS
```

CI/PokeTrade:

```text
build                     0 warnings / 0 errors
C# tests                  59 / 59 PASS
frontend tests            11 / 11 PASS
tool pack/install         PASS
WorkPlay knowledge build  PASS
PokeTrade .NET build      PASS
PokeTrade Angular build   PASS
PokeTrade live acceptance PASS
PokeTrade knowledge       PASS
```

Pinned Jellyfin:

```text
normal source build             PASS
workflow candidates             386
product features                116
canonical Markdown files        504
facts                           43,700
analysis mode                   project-semantic 43,700 / 43,700
single-file bundle parity       PASS
portable ZIP exact file set     PASS
portable ZIP byte parity        PASS
raw .pkc leak                   none
src/ source leak                none
artifact id                     10333522300
artifact sha256                 a96bc785c268da30ef03f4282eb64246c964c49599b7443d4020bd80fc049d85
```

## Re-review instructions

Do not treat green automation as sufficient by itself. Independently challenge:

```text
B6.1 custom Where / Any / First* / Single* authority
B6.2 nested initializer ownership
B6.3 duplicate method names across different Angular API services
conservative behavior when semantic/ownership proof is missing
absence of repository-specific hardcoding
PokeTrade + Loren + Jellyfin regressions
portable bundle/ZIP parity and no-leak
```

If any new blocker is found, return a concrete reproducer and keep V0.4.6 open.

Only an independent PASS may close V0.4.6.

## Scope locks

Do not start V0.4.7 before independent V0.4.6 PASS.

Do not start Azure DevOps ingestion. V0.5 ADO remains a later input-evidence source for Epic/Feature/PBI, acceptance intent, history/status and PR/commit linkage; it must not compensate for missing code-derived business logic.

## Bootstrap prompt for independent re-review

```text
Review current PKC main for V0.4.6 only.

Read:
1. docs/status.md
2. docs/handoff.md
3. docs/reviews/2026-09-14-v0.4.5-v0.4.6-independent-review.md
4. docs/reviews/2026-09-14-v0.4.6-independent-rereview-request.md

V0.4.5 is already accepted PASS.

Independently verify that B6.1, B6.2 and B6.3 are generically fixed, including adversarial counterexamples, conservative fallback, no benchmark hardcoding, cross-benchmark regression and portable parity/no-leak.

Do not review or start V0.4.7 or Azure DevOps.
Return PASS only if V0.4.6 has no unresolved blocker.
```
