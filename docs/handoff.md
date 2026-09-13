# PKC Handoff

Use this file when continuing PKC in another coding/review thread.

## Product contract

PKC is a Product/System Knowledge Compiler. A Product Owner should be able to hand generated portable knowledge to an AI assistant and ask product/system questions without requiring a source-code rescan.

Keep the compiler architecture deterministic:

```text
source
→ analyzers/adapters
→ evidence/facts
→ feature/workflow candidates
→ knowledge synthesis
→ canonical model
→ portable rendering
```

Do not implement direct source-to-freeform-AI generation.

## Current state

**V0.4.4 remains open. The B1 context-proof blocker from independent re-review 2 has a regression-first implementation fix. All current automated gates are green on the implementation checkpoint. Final independent re-review is required. V0.4.5 remains locked.**

Read in order:

```text
docs/status.md
docs/handoff.md
docs/reviews/2026-09-13-v0.4.4-external-review.md
docs/reviews/2026-09-13-v0.4.4-external-rereview.md
docs/reviews/2026-09-13-v0.4.4-external-rereview-2.md
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

B2 repeated-build parity, B3 capability-flow anti-overfit, and B4 frontend product-source scope remain independently PASS. Do not reopen them without a new regression.

## Independent re-review 2 blocker

`docs/reviews/2026-09-13-v0.4.4-external-rereview-2.md` found one remaining B1 sub-blocker: context roots were stripped more broadly than the evidence proved.

Required safety contract:

```text
Angular:
strip form-value scaffolding only for the form root proven by the current binding.

Backend:
strip a condition root only when it is the proven validated/request-object root,
not merely any endpoint parameter.

No deterministic proof:
return a conservative non-consistent classification; never consistent/high.
```

## Regression-first follow-up

Red commits:

```text
aa06bd7b21ab44952935046637b30c65a7f4b9ad  test: reproduce remaining validation proof gaps
1881bc860f9c77a016a852689655fab42ba58975  test: reject unbound identifier equality
```

The red run proved four context/equality gaps before the first hardening fix:

```text
frontend loose `==` was treated like strict `===`
unrelated Angular object `.value` path could be stripped as a form path
C# request root comparison could ignore identifier case
unbound path/path textual coincidence could be treated as cross-stack proof
```

First hardening fix:

```text
b4aba38d6908e45e488dd544cc5314454aaa0297  fix: require explicit validation equivalence proof
```

This made equality/context proof explicit:

- supported frontend equality requires `===`;
- Angular form scaffolding is removed only when the root exactly matches binding metadata `form`;
- endpoint parameter roots are case-sensitive on the C# side;
- path/path textual coincidence is unproven unless an explicit field/value rule succeeds first.

Author-side adversarial review then found two additional backend-root forms with the same underlying context-proof issue.

Red commit:

```text
5bb48c7b020ea66d6fd90f2cd14691bc9c27624a  test: bind backend condition to validated request root
```

Workflow `34758501605` failed exactly:

```text
Backend_unrooted_identifier_is_not_proven_as_request_field
Backend_other_endpoint_parameter_is_not_the_validated_request_context
```

while 51 prior C# tests and all 8 frontend tests remained green.

Final implementation fix:

```text
ab3bdf32c4076dcd22c957b122aea6cc424b027c  fix: bind backend condition to validated request root
```

For a backend condition operand to become a canonical request field, it now must:

```text
1. be a rooted member path;
2. use an exact-case endpoint parameter root;
3. if the validation fact exposes `parameterName`, use exactly that validated-object root.
```

This uses provenance already emitted by `CSharpValidationEvidenceScanner` for guard-condition validation facts. An unrooted identifier or another endpoint parameter is no longer enough proof.

## Preserved positive behavior

The hardening remains intentionally narrow. Supported proven cases still work, including:

```text
UI:      serviceType === 'CSP'
Backend: request.ServiceType == ServiceType.CSP

Angular UI with binding form `targetForm`:
targetForm.value.status === 'Active'
Backend:
request.Status == Status.Active
```

Compound conditions are still compared as complete canonical sets, and signed numeric, decimal, quoted punctuation, member-path segment identity/case and deterministic enum/string equivalence from the prior B1 work remain protected by regression tests.

## Current verified gates

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

Pinned Loren output:

```text
artifact id:      10318151788
artifact digest:  sha256:e28b6310f2cabb102c472e75175c5791e6ab825778e1d30616c496497ab2436f
structured files: 23
bundle parity:     23 / 23
ZIP parity:        23 / 23
missing content:    0
source/raw leak:     0
```

Direct artifact inspection confirmed parity between structured Markdown, the single-file bundle and the portable ZIP.

## Next action

Do **not** start V0.4.5 yet.

The next action is a **final independent B1 re-review of current main HEAD**. The reviewer should read the three external review records and verify at minimum:

```text
re-review-2 Angular wrong-root counterexample
re-review-2 backend unrelated-parameter counterexample
unrooted backend condition identifier
frontend loose equality
C# root case sensitivity
unbound cross-stack path/path coincidence
preservation of known positive field/enum and Angular bound-form cases
all current gates and portable handoff parity
```

Only if that independent review returns PASS may V0.4.4 close and V0.4.5 unlock.

## Scope locks

Until final independent B1 re-review passes, do not start:

- V0.4.5 second real-repository work;
- Azure DevOps ingestion;
- unrelated frontend/framework rewrites;
- browser/runtime exploration;
- generalized drift/insight work;
- other roadmap expansion.

Warnings W1 (claim-level portable provenance) and W2 (durable blind-review evidence) remain non-blocking follow-up concerns.

## Bootstrap prompt for final independent re-review

```text
Independently re-review PKC V0.4.4 B1 from current main HEAD.
Read docs/status.md, docs/handoff.md, docs/reviews/2026-09-13-v0.4.4-external-review.md, docs/reviews/2026-09-13-v0.4.4-external-rereview.md, and docs/reviews/2026-09-13-v0.4.4-external-rereview-2.md.
Focus only on validation-condition equivalence safety, especially proven Angular/backend context roots and conservative handling when proof is absent. Verify the new regressions, preservation of valid positive equivalence, all current gates, and handoff parity. Return PASS or concrete B1 blocking findings. Do not advance V0.4.5 unless this independent re-review passes.
```
