# Real-repository Product-value Scorecard — 2026-09-23

## Purpose

This benchmark answers a different question from the fail-closed safety benchmark:

> Is the generated knowledge actually useful for Product Owner / QC questions, and where does it lose known product behavior?

A green workflow, nonzero fact count or zero false-positive authority count is **not** sufficient for product acceptance.

## Exact production

```text
37a71172c8c425219aef35f5843ac2109810ae9d
fix: preserve Angular infrastructure render evidence
```

Standard exact-SHA verification:

```text
CI + full tests + WorkPlay + PokeTrade  35823346084 — PASS
pinned Loren                           35823346060 — PASS
Loren-main canary                      35823346077 — PASS
pinned Jellyfin                        35823346046 — PASS
Release build                          0 warnings / 0 errors
C#                                     253 / 253 PASS
frontend                               13 / 13 PASS
```

Pinned Jellyfin remains 43,365 facts / 195,316 relations / 386 workflows / 116 features / 504 Markdown, all 43,365 project-semantic, portable parity/no-leak PASS.

## Benchmark execution

```text
wrapper  f76c946e15cfe37d380745e0a2efb22cd3fc9123
parent   37a71172c8c425219aef35f5843ac2109810ae9d
run      35823872896 — PASS, 3 / 3
```

The wrapper differs from production only by the benchmark branch trigger.

Artifacts:

```text
agentic-angular  10734561505  sha256:b15120b5b17580ed9defe3600147ed0998f5cd9e121f8a6b6cc02297fec5daf4
jin12-crm        10734496662  sha256:36aa1772e18ebba7c3911429edbf51eb51c256f16065f0563b4c15d59b4ec8d1
kesetovic-crm    10733409752  sha256:d1f90d1cb1eb97d5ff6129134254bc11981faf28d78fd92fd9617a1a4f212fb0
```

Canonical `.pkc`, `knowledge/` and `PKC_KNOWLEDGE.md` evidence is byte-identical to the preceding safety benchmark for all three repositories. The current R7.10 repairs therefore close authority holes without perturbing this pinned real-repo corpus.

## Corpus coverage

| Repository | Facts | Relations | Workflows | Product features | UI→API workflows | Workflows with business rules | State-change workflows |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| Agentic Angular CRM | 441 | 660 | 18 | 6 | 15/18 | 17/18 | 4/18 |
| Jin12 CRM | 415 | 1,580 | 16 | 7 | 0/16 | 9/16 | 0/16 |
| Kesetovic CRM | 488 | 2,054 | 16 | 10 | 0/16 | 13/16 | 3/16 |

Additional observations:

- Agentic has concrete UI interaction steps for 5/18 workflows.
- Permissions are present in 15/18 Agentic, 14/16 Jin12 and 14/16 Kesetovic workflows.
- No workflow in the three artifacts currently has a grounded `Side effects` section.
- All three pinned repositories still yield zero authoritative R7.9/R7.10 positive cross-layer answers.

## Known-answer checks

Known answers are verified independently against the pinned source. They are not inferred from PKC output.

### Agentic Angular CRM

#### Login validation — PASS

Pinned source requires email/password, valid email, correct credentials and active user, then creates a token. Generated `auth/login.md` captures the four gates and `POST /api/v1/login/access-token` linkage.

#### Signup validation — PASS

Generated `auth/signup.md` captures required email/password, email validity, password 8–40 and unique email, plus UI→API linkage.

#### Signup-created defaults — FAIL

Pinned source creates a user with important defaults such as active/non-superuser state. Generated Signup reports no grounded state changes.

#### Auth feature synthesis — FAIL

Child workflows contain grounded validation rules, but `features/auth/operations.md` reports no grounded business rules.

### Kesetovic CRM

#### Pack order — PASS

Generated workflow captures Packer/Admin, existence, NEW-only precondition and transition to PACKED.

#### Complete order — PASS

Generated workflow captures Packer/Admin, existence, PACKED-only precondition and transition to COMPLETED.

#### Cancel order — PASS

Generated workflow captures Packer/Admin, existence, NEW/PACKED-only precondition and transition to CANCELLED.

#### AddOrder initial/computed state — FAIL

Pinned source creates the order with `OrderStatus = NEW` and `BonusAwarded = OrderPrice * 0.05`; generated AddOrder reports no grounded state changes and omits the 5% computation as product behavior.

#### SignalR side effect — FAIL

Pack/Complete/Cancel/AddOrder invoke `SendAsync("OrderSignal")`. PKC sees `SendAsync` in backend flow, but `Side effects` remains empty.

#### Order feature synthesis — FAIL

Workflow files contain precise status preconditions, but `features/order/status-management.md` reports no grounded business rules.

### Jin12 CRM

#### User-scoped contact listing — FAIL

Pinned `ContactService.GetAllAsync(userId)` calls `GetByUserIdAsync(userId)` so users see their own contacts. Generated workflow stops at `IContactService.GetAllAsync` and reports no grounded business rule.

#### Contact update — FAIL

Pinned service checks existence, updates contact fields, sets `UpdatedAt` and persists. Generated workflow captures controller exception handling but reports no state changes.

#### Contact delete — PARTIAL

Endpoint, authorization and controller-level not-found behavior are present, but concrete service behavior is not reconstructed into the workflow.

## Verdict

```text
Build / regression integrity            PASS
Real-repository execution               PASS
Fail-closed authority safety            PASS
Backend workflow usefulness             PARTIAL / PROMISING
Endpoint / permission discovery         GOOD
Direct controller preconditions         GOOD
Cross-method/interface reconstruction   NOT PASS
Construction/default/computation state  NOT PASS
Side-effect synthesis                   NOT PASS
Feature-summary fidelity                NOT PASS
UI → API usefulness                     PARTIAL / REPO-DEPENDENT
R7.14 positive cross-layer yield        NOT PASS
Overall product-value benchmark         NOT PASS
```

## Benchmark policy going forward

Every real-repository benchmark has two separate gates.

### Gate A — safety / regression

Require unchanged pinned source, honest target-build outcome, bounded PKC execution, no unsupported authority promotion, no portable raw/source leakage, accepted causality closure and canonical stability where expected.

### Gate B — product value / known answers

Maintain source-verified expected questions/assertions and score each PASS / PARTIAL / FAIL / N/A across:

1. endpoint/capability;
2. permissions;
3. business preconditions;
4. state transitions;
5. object defaults/computations;
6. side effects;
7. interface/concrete implementation behavior;
8. UI interaction;
9. UI→API linkage;
10. feature-summary fidelity;
11. cross-layer render/visibility proof;
12. explicit unknowns.

Zero positive cross-layer yield may pass Gate A but can never pass Gate B or R7.14.

## Product-value work exposed for E

After D is independently accepted, prioritize:

1. preserve grounded workflow rules when synthesizing product features;
2. traverse interface calls to proven concrete implementations;
3. capture construction-time defaults and computations;
4. promote grounded integration calls into product `Side effects`;
5. establish at least one unchanged real-project R7.14 positive without weakening authority;
6. encode this known-answer matrix as permanent automated benchmark assertions.

## Current conclusion

The current candidate is safe enough for manual pre-acceptance testing and already useful for many backend PO/QC questions, but **the real-repository product-value benchmark is not yet PASS**.
