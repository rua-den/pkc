# PKC Product-Value Benchmark Protocol

This protocol measures whether generated PKC knowledge is useful to a Product Owner / QA agent, not merely whether the compiler exits successfully.

A green build, successful scanner run, non-zero facts, or zero false-positive authority count is **not** product-value acceptance.

At the same time, the full AI benchmark is intentionally **not** run after every edit. AI rereads cost context/tokens and should be proportional to semantic risk.

## Privacy / source handling

Benchmarks may target proprietary repositories.

- Phase-1 product answering must use only the generated `.pkc/workspace`.
- Source inspection is allowed only in phase 2 and only inside the approved source-enabled/company environment.
- Do not copy source-code bodies, proprietary file contents, secrets, credentials, or raw fact payloads into benchmark reports.
- Reports may record behavior, endpoint names, source paths, symbol names, evidence classes and concise evidence descriptions.
- Never commit a proprietary target repository or its generated `.pkc/` directory into the PKC repository.
- PKC's generated workspace is intentionally source-free; environment/provider privacy is governed by the company's approved Claude Code/enterprise configuration, not by PKC itself.

## Three benchmark levels

### Level 0 — deterministic verification

**Default after every code change. No AI reread is required.**

Use focused/related tests and deterministic gates appropriate to the change, including when relevant:

- compile/build;
- regression tests;
- scanner/output schema assertions;
- authority/no-leak checks;
- workspace isolation;
- root `CLAUDE.md` / `AGENTS.md` preservation;
- path traversal rejection;
- expected generated files;
- output diff / idempotence;
- real-repository execution smoke.

A docs-only or implementation change that cannot affect product answers should normally stop at Level 0 after all required repository gates pass.

Examples:

```text
CLI help text
workspace directory plumbing
manifest formatting without semantic effect
CI/workflow maintenance
internal refactor with byte-identical canonical output
```

### Level 1 — targeted AI product-value benchmark

Run Level 1 when a change can materially change what a PO/QA agent may answer.

Typical triggers:

```text
interface → concrete implementation traversal
permission / precondition extraction
state mutation / default / computation recovery
side-effect synthesis
frontend event / service / URL linking
UI → API linkage
displayed-value lineage
feature/workflow aggregation
evidence authority that changes answerability
answer-routing/policy changes that materially alter responses
```

Select **only the affected repository/workflow and relevant questions**. One or two probes are normally sufficient.

Do not load an entire repository workspace when the catalog identifies a small relevant feature/workflow set.

### Level 2 — full AI product-value benchmark

Run the full pinned corpus and standard question set only for:

- milestone/product-acceptance checkpoints;
- release candidates;
- important demos;
- major canonical-knowledge model changes;
- major workspace/routing behavior changes;
- targeted results that indicate broad regression risk;
- explicit user/reviewer request.

Do not run Level 2 merely because a small implementation change landed.

## AI benchmark sequence — Level 1 and Level 2

Use the pinned real-repository corpus and any explicitly approved team-repository corpus named by the current handoff.

For each selected probe:

1. Run PKC from the exact candidate SHA.
2. Prefer the product UX:

   ```text
   pkc run <repository-path>
   ```

3. Start from `<repository>/.pkc/workspace`.
4. Read its generated `CLAUDE.md` or `AGENTS.md` first.
5. Follow `knowledge/START_HERE.md`, `_policy/answer-contract.md`, and `_meta/catalog.json`.
6. **Phase 1:** answer the selected PO/QA question using only generated workspace knowledge.
7. Record the generated answer before inspecting source.
8. **Phase 2:** in the approved source-enabled environment, inspect only enough pinned source to establish the known answer.
9. Compare generated answer to source-known answer and score it.
10. Report result, material misses and next repair without dumping source code.

Never modify a benchmark repository merely to make PKC produce a positive answer.
Never weaken evidence/authority rules merely to increase benchmark yield.

## Standard question set

For a representative workflow/capability, ask:

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

For Level 1, ask only the questions relevant to the changed semantic area.

If a question truly does not apply to that repository, mark it `N/A`; do not score it as a failure.

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

Level 1 reports only the selected dimensions plus concrete misses.

Level 2 reports at least:

```text
permission + preconditions
state changes / business effects
UI → API + displayed-value lineage
evidence trace
backend PO/QC core score
overall applicable score
```

Percentage alone is not enough. Always report the concrete miss that should drive the next implementation step.

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

These are stable probes, not the only allowed questions.

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

Use this primarily for permissions, direct state mutation, frontend→API, and displayed-value lineage.

### Jin12 — Contacts Update / GetAll

Source-known behavior includes:

- authorization at controller boundary;
- `GetAllAsync(userId)` returns contacts scoped to the current user through `GetByUserIdAsync(userId)`;
- concrete `ContactService.UpdateAsync` checks existence;
- updates FirstName, LastName, Email, Phone, JobTitle, CompanyId and UpdatedAt;
- persists through the contact repository.

Use this primarily for controller → interface → concrete implementation reconstruction.

### Kesetovic — PackOrder

Source-known behavior includes:

- roles Packer/Admin;
- target order must exist;
- only `NEW` may be packed;
- success changes status to `PACKED`;
- successful persistence sends SignalR `OrderSignal`;
- UI path is Pack button → emitted event chain → `OrderService.markPacked(id)` → `PUT /order/{id}/pack`.

Use this primarily for state transition, side effects and frontend event/service linkage.

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

Do not copy these numbers forward after a semantic change. Recompute only the Level-1 dimensions affected by the change, or all dimensions for Level 2.

## Acceptance interpretation

Keep two gates separate:

```text
Gate A: deterministic safety / fail-closed regression
Gate B: AI product value / known-answer answerability
```

Gate A PASS does not imply Gate B PASS.
Level 0 proves deterministic safety appropriate to the change.
Level 1 measures local semantic impact.
Level 2 measures broader product readiness.

## Handoff evidence

For a Level-0-only change, record:

- exact candidate SHA;
- focused/related/full deterministic gates used;
- why no AI benchmark was required.

For Level 1, additionally record:

- selected benchmark repo/workflow/questions;
- generated-workspace answer before source inspection;
- source-known-answer comparison without code dump;
- selected-dimension scores;
- concrete misses.

For Level 2, additionally record:

- all benchmark repo SHAs;
- full per-question/per-repository scores;
- aggregate score;
- remaining concrete misses;
- next highest-ROI repair.
