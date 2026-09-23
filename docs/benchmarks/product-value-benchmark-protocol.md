# PKC Mandatory Product-Value Benchmark Protocol

This protocol is mandatory after any change that can affect generated product knowledge, AI-workspace routing, feature/workflow synthesis, UI/API linkage, value lineage, permissions, state changes, side effects, or answerability.

A green build, a successful scanner run, non-zero facts, or a zero false-positive authority count is **not** product-value acceptance.

## Benchmark sequence

Use the pinned real-repository corpus and any explicitly approved team-repository corpus named by the current handoff.

For each repository:

1. Run PKC from the exact candidate SHA.
2. Prefer the product UX:

   ```text
   pkc run <repository-path>
   ```

3. Open only `<repository>/.pkc/workspace` as the AI product context.
4. Do **not** inspect source code first.
5. Let the generated `CLAUDE.md` / `AGENTS.md`, `knowledge/START_HERE.md`, catalog and answer contract route the answer.
6. Ask grounded Product Owner / QA questions from the generated workspace.
7. Record the generated answer before looking at source.
8. Only then inspect the pinned source repository to establish the known answer.
9. Compare generated answer to source-known-answer and score it.
10. Report per-question, per-repository and aggregate results.

Never modify a benchmark repository merely to make PKC produce a positive answer.
Never weaken evidence/authority rules merely to increase benchmark yield.

## Standard question set

For a representative workflow/capability in each repository, ask:

1. **Permission + preconditions**
   - Who may perform this action?
   - What conditions must be true before it can succeed?

2. **State / business effect**
   - What entity/data state changes after success?
   - Include grounded defaults, computations and state transitions when present.

3. **UI → API → displayed value**
   - Which UI action triggers the behavior?
   - Which API is called?
   - Where does the displayed value come from?

4. **Evidence trace**
   - What evidence supports the answer?
   - Is the evidence chain complete across the relevant frontend/backend layers?

If a question truly does not apply to that repository (for example a backend-only repository has no frontend), mark it `N/A`; do not score it as a failure.

## Scoring

Use explicit component scoring rather than a vague overall impression.

### Q1 — permission + preconditions

```text
50% permission/role/policy correctness
50% business precondition correctness
```

### Q2 — state / business effect

Score the fraction of independently verified state mutations, transitions, defaults and computations that the generated answer recovers without unsupported additions.

### Q3 — UI/API/value lineage

```text
50% UI action → API linkage
50% displayed-value origin / value lineage
```

### Q4 — evidence trace

Score completeness of the grounded evidence chain needed for the answer. A source path alone is not a complete trace if material intermediate behavior is missing.

For each answer also label:

```text
PASS     = materially complete and correct
PARTIAL  = useful but misses material known behavior
FAIL     = unavailable, wrong, or materially misleading
N/A      = not applicable to this repository
```

Exclude `N/A` from percentage denominators.

## Report dimensions

Always report at least:

```text
permission + preconditions
state changes / business effects
UI → API + displayed-value lineage
evidence trace
backend PO/QC core score
overall applicable score
```

Also report concrete misses that drive the next implementation step. Percentage alone is not enough.

## Current pinned corpus

Use exact pinned source SHAs unless the active handoff explicitly changes them:

```text
hackersandwizards/agentic-engineering-training-angular
22f2aab64617f4de7984370a5bd40e8c9535dbf5

jin12-xyz/CRM
00493af54d4d9e146d1c6eb75f5dc8f3898f09ec

kesetovic/crm-system
8e3b74bec4fdcd0144bd65f0c1b49c8e801bd2f7
```

## Current known-answer probes

These are useful stable probes; they are not the only allowed questions.

### Agentic — Users Update

Source-known behavior includes:

- Superuser authorization;
- target user must exist;
- optional email must be valid and unique;
- optional password length is 8–40;
- direct updates to Email, Password hash, FullName, IsSuperuser and IsActive;
- modified users receive `UpdatedAt` through DbContext timestamp handling;
- Angular edit dialog initializes form fields from injected user data;
- frontend service sends `PATCH /api/v1/users/${id}`.

### Jin12 — Contacts Update / GetAll

Source-known behavior includes:

- authorization at controller boundary;
- `GetAllAsync(userId)` returns contacts scoped to the current user through `GetByUserIdAsync(userId)`;
- concrete `ContactService.UpdateAsync` checks existence;
- updates FirstName, LastName, Email, Phone, JobTitle, CompanyId and UpdatedAt;
- persists through the contact repository.

This probe intentionally exposes controller → interface → concrete implementation reconstruction quality.

### Kesetovic — PackOrder

Source-known behavior includes:

- roles Packer/Admin;
- target order must exist;
- only `NEW` may be packed;
- success changes status to `PACKED`;
- successful persistence sends SignalR `OrderSignal`;
- UI path is Pack button → `onPack()` → emitted event → `handleEventChange()` → `OrderService.markPacked(id)` → `PUT /order/{id}/pack`.

This probe intentionally tests backend correctness plus frontend event/service linkage.

## Current baseline

Reference report:

`docs/benchmarks/2026-09-23-ai-question-answerability-benchmark.md`

Current pre-workspace baseline was approximately:

```text
Agentic Users Update       80.8%
Jin12 Contacts Update      40.0%
Kesetovic PackOrder        65.0%

permission/preconditions   90.0%
state changes              61.0%
UI/API/value lineage       25.0%
evidence trace             66.7%
backend PO/QC core         72.6%
overall applicable         63.9%
```

Do not treat these percentages as permanent targets or truth after source/corpus changes. Recompute from the candidate output.

## Acceptance interpretation

Keep two gates separate:

```text
Gate A: safety / fail-closed regression
Gate B: product value / known-answer answerability
```

Gate A PASS does not imply Gate B PASS.
A benchmark with zero useful positive answers may be safe, but it is not a successful product benchmark.

After each meaningful product-knowledge fix, the handoff must record:

- exact candidate SHA;
- exact benchmark repo SHAs;
- commands/run IDs used;
- generated-workspace answer before source inspection;
- source-known-answer comparison;
- per-question scores;
- aggregate score;
- remaining concrete misses;
- next highest-ROI repair.
