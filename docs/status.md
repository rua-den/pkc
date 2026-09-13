# PKC Status

Last updated: 2026-09-13

## Current milestone

**V0.4.4 Loren Knowledge Readiness — B1 CONTEXT-PROOF FIX IMPLEMENTED / FINAL INDEPENDENT RE-REVIEW REQUIRED.**

Latest independent review:

```text
docs/reviews/2026-09-13-v0.4.4-external-rereview-2.md
```

That review failed candidate `f98f05a624fe4262615db992fbe3f9ea3bd43bce` on one remaining B1 sub-blocker: Angular/backend context roots could still be stripped without enough proof.

The blocker is now fixed regression-first. Current implementation checkpoint:

```text
ab3bdf32c4076dcd22c957b122aea6cc424b027c
```

Review lineage:

```text
original review HEAD:      34f77c036206d48bbf9495ea73e5debcea9f0eb3
all-four-fix checkpoint:   8b01d4b5ba84112f36e46b234426f754be38c8f6
first re-review HEAD:       e022dbf581846c4699b724df063cd68b47dff332
B1 typed-operand HEAD:      5fba7fd08e62e721e96e5a93bc65ab153d663f66
second re-review HEAD:      f98f05a624fe4262615db992fbe3f9ea3bd43bce
context-proof fix HEAD:     ab3bdf32c4076dcd22c957b122aea6cc424b027c
```

V0.4.3 remains the last accepted package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

Do **not** mark V0.4.4 PASS and do **not** advance V0.4.5 until an independent reviewer accepts the current B1 candidate.

## Blocker disposition

```text
B1 typed operand semantics                         IMPLEMENTED / PRIOR REVIEW PASS
B1 complete condition-set comparison              IMPLEMENTED / PRIOR REVIEW PASS
B1 proven context stripping                        FIX IMPLEMENTED / RE-REVIEW REQUIRED
B2 repeated-build canonical parity                 PASS
B3 capability-flow anti-overfit                    PASS
B4 frontend product-source scope                   PASS
```

B2/B3/B4 were not reopened.

## B1 context-proof regression-first evidence

The independent re-review required two context-root regressions. Author-side adversarial review found two additional paths with the same safety property, and all were locked before the implementation fix.

First red hardening commits:

```text
aa06bd7b21ab44952935046637b30c65a7f4b9ad  test: reproduce remaining validation proof gaps
1881bc860f9c77a016a852689655fab42ba58975  test: reject unbound identifier equality
```

On `1881bc8...`, the test gate failed exactly four new B1 cases while 47 existing C# tests and all 8 frontend tests remained green:

```text
frontend loose `==` vs backend equality                         NOT proven
unrelated Angular `<object>.value.<field>`                      NOT form scaffolding
case-mismatched C# request parameter root                       NOT the same identifier
unbound path/path textual coincidence across stacks             NOT proven
```

First fix:

```text
b4aba38d6908e45e488dd544cc5314454aaa0297  fix: require explicit validation equivalence proof
```

It:

- requires strict frontend `===` for supported UI equality proof;
- strips Angular form scaffolding only when the root exactly matches the current binding's known `form` metadata;
- treats C# endpoint parameter names case-sensitively;
- rejects generic path/path textual coincidence instead of treating matching names as cross-stack proof.

A second author-side context review then found two backend-root ambiguities and locked them before the final fix:

```text
5bb48c7b020ea66d6fd90f2cd14691bc9c27624a  test: bind backend condition to validated request root
```

Workflow run `34758501605` failed exactly the two new regressions while 51 existing C# tests and all 8 frontend tests remained green:

```text
backend unrooted identifier `Status`                            NOT proven request state
other endpoint parameter `cancellationToken.Status`             NOT validated request state
```

Final implementation fix:

```text
ab3bdf32c4076dcd22c957b122aea6cc424b027c  fix: bind backend condition to validated request root
```

Backend condition-field proof now requires:

```text
member path has an explicit root
AND root is an exact-case endpoint parameter
AND, when validation evidence carries `parameterName`,
    root exactly equals that validated-object root
```

If those conditions are not proven, equivalence is conservative and cannot become `consistent / high`.

## Current automated evidence

For implementation checkpoint `ab3bdf32c4076dcd22c957b122aea6cc424b027c`:

```text
full CI / WorkPlay          PASS  run 34758617653
  C# tests                  53 / 53 PASS
  frontend tests             8 / 8 PASS
  build                      0 warnings / 0 errors
PokeTrade real system       PASS  run 34758617653
pinned Loren external       PASS  run 34758617664
Loren-main canary           PASS  run 34758617742
```

Pinned Loren artifact:

```text
artifact id:      10318151788
artifact digest:  sha256:e28b6310f2cabb102c472e75175c5791e6ab825778e1d30616c496497ab2436f
structured files: 23
bundle parity:     23 / 23
ZIP parity:        23 / 23
missing content:    0
source/raw leak:     0
```

Direct artifact inspection confirmed every structured Markdown file is present verbatim in `PKC_KNOWLEDGE.md`, the inner `PKC_KNOWLEDGE.zip` contains the same 23 `knowledge/` Markdown files, and no `.pkc/` or source files leak into the portable ZIP.

## Acceptance position

The implementation author-side precheck has no remaining B1 finding in the reviewed supported equality/context surface, and all current automated gates are green.

This is **not** an independent acceptance decision. Exact next action:

```text
independent final B1 re-review on current main HEAD
→ verify the re-review-2 context-root blocker and new conservative regressions
→ if PASS, close V0.4.4
→ only then unlock V0.4.5
```

Warnings W1 (claim-level portable provenance) and W2 (durable blind-review evidence) remain non-blocking and outside this B1-only scope.
