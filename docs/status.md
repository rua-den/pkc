# PKC Status

Last updated: 2026-09-14

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             IMPLEMENTATION GREEN / INDEPENDENT RE-REVIEW PENDING
V0.4.7 cross-layer PO-question readiness         LOCKED
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.6 is **not complete yet**. B6.1, B6.3 and B6.4 are implemented and green across the current automation gates, but the milestone still requires an independent adversarial re-review before acceptance.

Current implementation checkpoint under re-review:

```text
34c3bf00233ac4c0c4df717f7f7c5ae55fffe07b
fix: preserve observable predicate return semantics
```

Latest failed independent review:

```text
docs/reviews/2026-09-14-v0.4.6-independent-rereview.md
reviewed code HEAD: 45b2d9b6e215f26c395843c77f1e59887774e21b
```

Current re-review request:

```text
docs/reviews/2026-09-14-v0.4.6-independent-rereview-request.md
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

## V0.4.6 blocker status

### B6.1 — IMPLEMENTED + GREEN / independent re-review pending

Independent review found that same-line custom and real LINQ invocations could share line-level semantic authority.

Focused regression:

```text
Same_line_custom_Where_cannot_borrow_real_Linq_Where_semantic_authority
```

Current authority path re-resolves the exact predicate invocation using its syntax span start in the real project semantic model and accepts only the supported target:

```text
System.Linq.Enumerable.<operation>
System.Linq.Queryable.<operation>
```

If exact invocation authority cannot be proved, the authoritative business-predicate fact is rejected.

Regression-first history:

```text
RED commit   cf7645eef7b22d82803ea9f48e7575677b8d2a07
RED run      34813594610 — FAIL expected
FIX commit   d784013b54acbec22dc9c18a3cfa738b38adc3d3
GREEN run    34813887033 — PASS
```

### B6.2 — PASS / keep closed

Nested property object initializers are not promoted as extra configured items of the source collection for the reviewed supported forms. Direct configured items remain grounded with direct ownership provenance.

Do not reopen B6.2 without a new concrete contradiction.

### B6.3 — IMPLEMENTED + GREEN / independent re-review pending

Independent review found that Angular result/list correlation used simple service class names, allowing same-named classes from different modules to cross-link.

Focused regression:

```text
Same_service_class_name_in_different_modules_does_not_cross_link_list_flow
```

Implemented property:

```text
API owner identity = source module path + exported class
component service identity = deterministically resolved relative import module + exported class
named import aliases normalize to exported class identity
cross-stack result/list correlation requires exact deterministic service identity equality
unresolved service module identity does not produce authoritative correlation
```

Regression-first history:

```text
RED commit   df1ea834b970201b511c8ff9a975f4e1fe59c53d
RED run      34814066361 — FAIL expected
FIX commit   84da527e8341918b1803e36ccd177a4d14056cd8
GREEN run    34814396514 — PASS
```

### B6.4 — IMPLEMENTED + GREEN / independent re-review pending

Independent review found that a genuine LINQ predicate could be promoted to an endpoint business rule merely because it was reachable, even when its result did not affect the observable endpoint outcome. It also found polarity risk for `Any`/`All`.

Implemented distinction:

```text
observed local query predicate
!=
proven observable business rule
```

Current behavior:

```text
exact supported LINQ invocation is still retained as evidence
non-observable predicate authority = observed-only
non-observable relation            = observes-predicate
observable rule promotion requires direct deterministic return/yield-return participation
observable metadata                = businessRuleAuthority=observable
                                     observableEffectResolution=direct-return-syntax
```

Focused regressions now cover:

```text
1. local Where result used only on a side path does not become an authoritative inclusion rule
2. Any guard that throws is not rendered as a positive existence requirement
3. returned Where remains an authoritative inclusion rule
4. direct Any return is rendered as "Returns whether ...", not "Requires ..."
5. direct All return is rendered as "Returns whether ...", not "Requires ..."
```

Regression/fix history:

```text
B6.4 observable-authority RED run     34817523642 — expected failures
observable-authority fix commit       304a39053b243bceb6c39d85f47cc7fab68fd57f
Any/All polarity RED commit           39079f0b7a8f9abccc850f3464202f57f9caa220
Any/All polarity RED run              34817972557 — 2 expected failures
final B6.4 fix commit                  34c3bf00233ac4c0c4df717f7f7c5ae55fffe07b
```

## Exact implementation-checkpoint automation

All push-triggered gates for implementation checkpoint `34c3bf00233ac4c0c4df717f7f7c5ae55fffe07b` are green:

```text
CI + PKC tests + WorkPlay + PokeTrade   34841796973 — PASS
pinned Loren                            34841796981 — PASS
Loren-main canary                       34841796980 — PASS
pinned Jellyfin                         34841797032 — PASS
portable parity / provenance check      PASS (inside Jellyfin run)
```

The CI run independently completed both jobs successfully:

```text
test                  PASS
poketrade-real-system PASS
```

PokeTrade gate includes backend build, Angular build, live business-branch acceptance and generated product-knowledge verification.

Jellyfin gate includes source build, PKC knowledge compilation, portable handoff parity/provenance verification and artifact upload.

Current Jellyfin artifact:

```text
artifact id:     10346636986
artifact digest: sha256:5a643ec5d2ed1f84af2a70d2b0cd108a2ca13500b6c064f8a615d4481f49b987
```

## Exact next action

Stay in V0.4.6 and perform **independent adversarial re-review only** for B6.1, B6.3 and B6.4 against implementation checkpoint:

```text
34c3bf00233ac4c0c4df717f7f7c5ae55fffe07b
```

The reviewer must challenge the implementation, not merely trust green automation. In particular:

```text
B6.1 — same-line/custom same-named invocation identity
B6.3 — same class + same method name across different Angular modules/import aliases
B6.4 — non-observable LINQ predicates, return participation, guard polarity, Any/All/selection wording
```

If the independent re-review finds a concrete blocker, reproduce it regression-first and fix only that blocker.

Only after an independent V0.4.6 PASS may V0.4.7 begin.

Do not start Azure DevOps ingestion.
