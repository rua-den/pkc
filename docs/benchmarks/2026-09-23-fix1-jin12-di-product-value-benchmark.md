# Fix #1 Jin12 DI Product-Value Benchmark

Date: 2026-09-23

Status: **PASS for the bounded direct-DI interface → concrete proof path**

## Scope

This is the Level-1 product-value validation for Fix #1:

```text
controller / interface call
→ exact Roslyn interface method
→ proven host DI registration
→ exact concrete implementation method
→ concrete guards / mutations / downstream calls
```

This report intentionally does not claim support for factory delegates, decorators, assembly scanning, keyed services, helper registration indirection, conditional registrations, or other unsupported DI shapes.

## Pinned inputs

```text
PKC production SHA
1401d42291f5da4ce59ba79887ac53106b978dd7
fix: prove DI dispatch host authority

benchmark wrapper branch
benchmark/fix1-jin12-1401

benchmark wrapper commit
ed498f63578444e4a13ea5a31957d2597890f2f8
test: run targeted Jin12 DI benchmark

workflow run
35862459255

target repository
jin12-xyz/CRM

target SHA
00493af54d4d9e146d1c6eb75f5dc8f3898f09ec
```

The target repository was not modified.

The benchmark wrapper executed `pkc run <repo>` and uploaded only `.pkc/workspace/` for phase 1.

## Questions

Selected workflow: **Contacts Update**

Q1. Who may perform the update, and what preconditions apply?

Q2. What state/data changes when the update succeeds?

Q4. What evidence trace supports the answer?

Baseline before Fix #1:

```text
Q1          70%
Q2           0%
Q4          50%
repo score  40%
```

## Phase 1 — workspace only

Phase 1 used only the generated Jin12 `.pkc/workspace`. Source was not inspected until the answer and score below were frozen.

### Frozen Q1 answer

The update requires authorization. The concrete update behavior also requires the target contact to exist; if the contact lookup produces no contact, the update does not proceed and the controller exposes the not-found outcome.

The workspace did not prove a narrower role or policy than authorization, so none was invented.

**Score: 100%**

### Frozen Q2 answer

On a successful update the concrete behavior changes:

- `FirstName`
- `LastName`
- `Email`
- `Phone`
- `JobTitle`
- `CompanyId`
- `UpdatedAt`

The backend flow also shows the concrete service calling the contact repository update operation after those mutations, supporting persistence through the repository.

**Score: 100%**

### Frozen Q4 answer

The workspace contains a trace from:

```text
PUT /api/Contacts/{id}
→ ContactsController.Update
→ IContactService.UpdateAsync
→ direct DI registration in CRM.API/Program.cs
→ ContactService.UpdateAsync
→ existence guard
→ field mutations
→ contact repository GetByIdAsync / UpdateAsync
→ response mapping
```

Workspace evidence locations include:

- `CRM.API/Controllers/ContactController.cs:L58-L71`
- `CRM.Application/Services/IContactService.cs:L15`
- `CRM.API/Program.cs:L32`
- `CRM.Application/Services/ContactService.cs:L94-L109`
- `CRM.Application/Services/ContactService.cs:L154-L165`

**Score: 100%**

### Frozen phase-1 score

```text
Q1          100%
Q2          100%
Q4          100%
repo score  100%
```

At this point source remained unopened for this benchmark phase.

## Phase 2 — pinned source cross-check

After phase 1 was frozen, the minimum required source at the pinned Jin12 SHA was inspected to establish the known answer.

Cross-checked symbols and locations:

- `CRM.API.Controllers.ContactsController.Update` — `CRM.API/Controllers/ContactController.cs:L58-L71`
- controller authorization — `CRM.API/Controllers/ContactController.cs`
- `CRM.Application.Services.IContactService.UpdateAsync` — `CRM.Application/Services/IContactService.cs:L15`
- direct `IContactService → ContactService` registration — `CRM.API/Program.cs:L32`
- built host for that startup builder — `CRM.API/Program.cs`
- `CRM.Application.Services.ContactService.UpdateAsync` — `CRM.Application/Services/ContactService.cs:L94-L109`
- response mapping — `CRM.Application/Services/ContactService.cs:L154-L165`
- repository update implementation persists through its data context — `CRM.Infrastructure/Repositories/GenericRepository.cs`, `UpdateAsync`

No raw proprietary/source snippets are reproduced here.

### Source-known behavior

The cross-check confirms:

1. the update endpoint requires authorization;
2. the controller delegates through `IContactService`;
3. the running host directly registers `IContactService` to `ContactService`;
4. the concrete service first loads the contact and rejects the missing-contact case;
5. a successful update assigns `FirstName`, `LastName`, `Email`, `Phone`, `JobTitle`, `CompanyId`, and `UpdatedAt`;
6. the service persists the mutated contact through the contact repository;
7. the controller returns the mapped successful result and exposes not-found for the missing-contact path.

No additional source fact within Q1/Q2/Q4 contradicted the frozen workspace answer.

## Score

| Question | Baseline | Fix #1 | Delta |
| --- | ---: | ---: | ---: |
| Q1 — permission / preconditions | 70% | 100% | +30 pp |
| Q2 — successful state changes | 0% | 100% | +100 pp |
| Q4 — evidence trace | 50% | 100% | +50 pp |
| **repo score** | **40%** | **100%** | **+60 pp** |

This is a targeted Level-1 score. It does not replace the full Agentic + Jin12 + Kesetovic Level-2 baseline.

## Concrete business facts recovered by Fix #1

The material recovery is the behavior that previously lived behind the interface boundary:

- target contact must exist;
- `FirstName` is updated;
- `LastName` is updated;
- `Email` is updated;
- `Phone` is updated;
- `JobTitle` is updated;
- `CompanyId` is updated;
- `UpdatedAt` is refreshed;
- the updated contact is passed to the contact repository for persistence;
- the evidence trace now identifies why the interface call can be attributed to `ContactService.UpdateAsync`: a specific supported direct DI registration on the host builder that is actually built.

The benchmark therefore demonstrates product value, not merely a new internal relation type.

## Safety / proof boundary

The production implementation remains intentionally fail-closed.

Proven authoritative scope:

- Roslyn semantic identities are assembly-qualified;
- registration lookup is project-scoped;
- only exact interface member/signature implementation is accepted;
- registration must be unique;
- supported direct forms are `AddScoped<I,T>`, `AddTransient<I,T>`, and `AddSingleton<I,T>`;
- the registration must occur at a supported top-level startup site on the host builder's `.Services`;
- the same supported builder must later be `.Build()`-ed;
- candidate traversal follows an explicit `dispatches` proof edge while retaining the original interface invocation evidence.

Negative regression coverage keeps dispatch closed for:

- ambiguous registrations;
- conditional registrations;
- dead/uninvoked helpers;
- unrelated service collections;
- never-built builders;
- unrelated hosts using the same contract assembly;
- same-named service types from unrelated projects.

Still unsupported by design:

- factory delegates;
- decorators;
- assembly scanning;
- keyed services;
- helper/extension registration indirection;
- other startup/host shapes without deterministic proof.

These are not benchmark failures for Fix #1; they remain explicit proof boundaries and must not be guessed.

## Remaining product-value gaps

This benchmark does not prove:

- frontend action → API linkage for Kesetovic PackOrder;
- displayed-value lineage for Agentic Users Update;
- a new overall three-repository score;
- formal V0.4.7-D/R7.10 acceptance.

The workspace's prose `Side effects` section is still sparse for this workflow even though backend flow exposes repository persistence. That is a synthesis-quality opportunity, not a blocker to the selected Q1/Q2/Q4 result.

## Decision

Fix #1 materially recovered the missing concrete implementation behavior on the pinned Jin12 workflow while preserving the intended fail-closed boundary.

**Decision: PASS / COMPLETE for the bounded direct-DI proof path.**

Fix #2 is unlocked:

**Frontend service URL expression resolution**, targeted first at Kesetovic `PackOrder`.

Initial scope remains string concatenation, template literals, known constants / proven `this.property` values, route-template normalization and backend route matching. Do not automatically broaden to `HttpParams`, `HttpHeaders`, Angular event propagation or unrelated dataflow without a targeted regression.
