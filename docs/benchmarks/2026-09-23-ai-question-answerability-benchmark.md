# PKC AI Question-Answerability Benchmark

Date: 2026-09-23

## Executive result

PKC production source used by this benchmark is already on `main`.

- Main/docs HEAD: `c3de07222cd5218c78368c71d28447a1600c5e98`
- Exact production source parent: `96205a9a643864facaf9642a3b390ddcdbed59d9`
- Benchmark wrapper: `65c02df67938197d929d30793dc012dbc3878ca3`
- Benchmark run: `35833147258`
- Wrapper delta from production: one workflow branch-trigger line only
- Real repositories: 3
- Benchmark jobs: 3/3 PASS

No production source exists only on the benchmark wrapper branch. `main` is the source of truth.

## Question set

The generated PKC knowledge pack was treated as the only knowledge source when answering these questions:

1. **Who may perform this workflow and what preconditions apply?**
2. **What entity state changes after this action succeeds?**
3. **Which UI action calls which API, and where does the displayed value come from?**
4. **Show the evidence trace supporting the answer.**

The answers were then compared against independently verified behavior from the pinned source repositories.

## Scoring

- Q1: 50% permission + 50% preconditions
- Q2: fraction of known state mutations/defaults/computations recovered
- Q3: 50% UI→API + 50% displayed-value lineage
- Q4: evidence-chain completeness, including relevant frontend/backend layers

`N/A` is excluded from the denominator.

## Score summary

| Probe | Q1 | Q2 | Q3 | Q4 | Repo score |
| --- | ---: | ---: | ---: | ---: | ---: |
| Agentic — Users Update | 100% | 83% | 50% | 90% | **80.8%** |
| Jin12 — Contacts Update | 70% | 0% | N/A | 50% | **40.0%** |
| Kesetovic — PackOrder | 100% | 100% | 0% | 60% | **65.0%** |

### Aggregate

| Dimension | Match |
| --- | ---: |
| Permission + precondition | **90%** |
| State changes | **61%** |
| UI → API + displayed-value lineage | **25%** |
| Evidence trace | **66.7%** |
| Core backend PO/QC questions (Q1 + Q2 + Q4) | **72.6%** |
| Overall applicable probes | **63.9% ≈ 64%** |

Safety and usefulness are separate:

- Safety / fail-closed benchmark: **PASS**
- AI product-question usefulness: **64% on this question set**
- Product acceptance: **NOT PASS**

---

# 1. Agentic Angular CRM — Users Update

## Generated answer from PKC pack

### Q1 — Who may update a user and what preconditions apply?

PKC can answer:

- Policy: `Superuser`
- target user must exist;
- if email is provided it must be valid;
- email must not already belong to another user;
- if password is provided its length must be 8–40;
- optional full name, superuser and active flags are applied only when supplied.

**Score: 100%**

### Q2 — What state changes?

PKC reports:

- `user.Email`
- hashed `user.Password`
- `user.FullName`
- `user.IsSuperuser`
- `user.IsActive`

Source also changes `User.UpdatedAt` from `AppDbContext.SetTimestamps()` on modified users.

PKC intentionally does not promote that transitive timestamp mutation because caller-object causality is not fully proven.

Recovered known entity mutations: 5/6.

**Score: 83%**

### Q3 — Which UI action calls which API, and where does the displayed value come from?

PKC reports:

- UI action: button on `EditUserDialogComponent`
- API: `PATCH /api/v1/users/${id}`

But it does not prove that the displayed edit-form values are initialized from injected `MAT_DIALOG_DATA` / `User` fields (`this.data.email`, `this.data.full_name`, etc.).

**Score: 50%**

### Q4 — Evidence trace

PKC gives exact evidence for:

- frontend button;
- frontend API service call;
- backend endpoint;
- Superuser policy;
- validations;
- direct state mutations.

The trace is strong but misses the displayed-value origin and the timestamp mutation.

**Score: 90%**

### Agentic verdict

**80.8% — demo-good.**

This is the best corpus for showing PKC today because it demonstrates frontend→API→backend workflow reconstruction plus permissions, rules and direct state mutation.

---

# 2. Jin12 CRM — Contacts Update

This repository is backend-only, so Q3 is not applicable.

## Generated answer from PKC pack

### Q1 — Who may update a contact and what preconditions apply?

PKC reports:

- authorization is required;
- controller catches `KeyNotFoundException` and returns NotFound.

Source behavior is stronger:

- concrete `ContactService.UpdateAsync` first loads the contact;
- missing contact throws `KeyNotFoundException`;
- otherwise update proceeds.

PKC exposes the outcome but fails to traverse the interface call into the concrete service implementation.

**Score: 70%**

### Q2 — What state changes?

PKC says:

`No grounded information available yet.`

Source changes:

- FirstName
- LastName
- Email
- Phone
- JobTitle
- CompanyId
- UpdatedAt

Recovered: 0/7.

**Score: 0%**

### Q3 — UI→API / displayed-value lineage

The pinned repository has no frontend project.

**Score: N/A**

### Q4 — Evidence trace

PKC traces:

`ContactsController.Update → IContactService.UpdateAsync`

but stops before:

`ContactService.UpdateAsync → repository → concrete mutations`

The evidence is real but materially incomplete for the business answer.

**Score: 50%**

### Jin12 verdict

**40.0% — not demo-good for deep business behavior.**

This is an excellent negative benchmark because it exposes the current interface→implementation traversal gap.

---

# 3. Kesetovic CRM — PackOrder

## Generated answer from PKC pack

### Q1 — Who may pack an order and what preconditions apply?

PKC reports:

- roles: `Packer`, `Admin`;
- order must exist;
- order status must be `NEW`;
- persistence must succeed.

This matches source behavior.

**Score: 100%**

### Q2 — What state changes?

PKC reports:

`order.OrderStatus = OrderStatus.PACKED`

This is the primary entity state mutation in the action.

**Score: 100%**

### Q3 — Which UI action calls which API, and where does the displayed value come from?

Source has a real UI path:

`Pack button`
→ `OrderDumbCardComponent.onPack()`
→ output event
→ `OrdersPackComponent.handleEventChange()`
→ `OrderService.markPacked(id)`
→ `PUT .../order/{id}/pack`

PKC workflow says:

- no grounded UI entry point;
- no grounded UI→backend linkage.

The pack also does not reconstruct displayed order-card values from the `order` input.

**Score: 0%**

### Q4 — Evidence trace

PKC strongly traces backend behavior:

- controller endpoint;
- role;
- existence/status conditions;
- state mutation;
- repository/unit-of-work calls.

But it misses:

- the real UI event chain;
- the frontend service call;
- `SignalR SendAsync("OrderSignal")` as a synthesized Side effect.

**Score: 60%**

### Kesetovic verdict

**65.0% — good backend demo, poor end-to-end demo.**

---

# What can be shown confidently today

## Strong demo claims

PKC can already demonstrate:

1. discovering backend capabilities/endpoints;
2. identifying authorization roles/policies;
3. extracting direct business preconditions;
4. reconstructing direct state transitions;
5. linking UI→API in repositories whose frontend call shape is currently recognized;
6. producing source evidence locations for grounded claims;
7. explicitly refusing to invent information when proof is missing.

## Claims to avoid today

Do not claim that PKC already reconstructs all of:

- interface→implementation business behavior;
- constructor/default/computed state;
- integration side effects;
- feature-summary business-rule preservation;
- arbitrary frontend HTTP call shapes;
- full displayed-value lineage;
- positive real-project R7.9/R7.10 cross-layer proof.

---

# Highest-ROI fixes exposed by the question benchmark

1. **Interface → concrete implementation traversal**
   - Biggest single improvement for Jin12-style architectures.
   - Turns `40%` backend workflow answers into materially useful product answers.

2. **Frontend service URL expression resolution**
   - Kesetovic uses `this.baseUrl + 'order/' + id + '/pack'`.
   - Current frontend scanner misses this linkage.
   - This directly attacks the current Q3 score of `25%`.

3. **Displayed-value lineage**
   - Example: `MAT_DIALOG_DATA → this.data.email → form control → displayed field`.
   - Required for the original PKC product vision, not just endpoint mapping.

4. **Construction/default/computation state**
   - Example: `OrderStatus = NEW`, `BonusAwarded = OrderPrice * 0.05`.

5. **Side-effect synthesis**
   - Example: `SignalR SendAsync("OrderSignal")`.

6. **Feature-level aggregation fidelity**
   - Do not lose rules already proven in child workflows.

---

# Show-off recommendation

For a live demo today:

### Use Agentic first

Ask:

> Who can update a user, what validations apply, what state changes, and which API is called from the UI?

Expected quality: **~81%**.

### Use Kesetovic second

Ask:

> Who can pack an order, from which state, and what state does it transition to?

Expected quality for this backend-specific question: **100%**.

Then deliberately ask:

> Which UI button triggers PackOrder and trace it to the API.

Expected result today: **FAIL / unknown**.

That failure is useful to show the benchmark is not self-congratulatory: PKC is designed to say “not proven” rather than fabricate a path.

---

# Bottom line

**Current realistic product-question score: ~64%.**

**Backend PO/QC core score: ~73%.**

**Best current demo corpus: Agentic (~81%).**

The product is already demonstrable, but the next engineering work should target the benchmark gaps rather than add more raw fact extraction.
