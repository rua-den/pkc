# Product-value Level 2 + Fix #4 prework checkpoint — 2026-09-23

## Scope

This report records product-value evidence collected after the three bounded product-value repairs while keeping formal V0.4.7-D / R7.10 acceptance separate.

Formal V0.4.7-E remains locked until independent rereview #17 accepts D. Nothing in this report self-certifies D or starts E.

## Production code checkpoint

```text
79b0f9fec80a2afb87f43ec7a559a5d54cb87863
fix: surface pkc run progress
```

The progress-log change is CLI UX only. Runtime CI confirmed phase logs for C# scan, frontend scan, linking, synthesis and workspace generation without changing artifact semantics.

## Fix #3 — targeted Agentic Level 1

Pinned target:

```text
hackersandwizards/agentic-engineering-training-angular
22f2aab64617f4de7984370a5bd40e8c9535dbf5
```

Benchmark wrapper:

```text
branch  benchmark/fix3-agentic-79b0
commit  eb8afb30230f3f2dd00eb9ff9342a95fb465a540
run     35899560821 — PASS
```

Phase-1 artifact contained `.pkc/workspace` only:

```text
artifact 10767823931
sha256   12c5f02760e0c0cc6f86e4232cc6a8d93df35e36a64c4349849a462ef9ab69c0
```

Workspace-only answer before source cross-check proved:

```text
MAT_DIALOG_DATA.data.email
→ EditUserDialogComponent.form.email
→ EditUserDialogComponent.displayed.email
```

Targeted result:

```text
Q3 UI/API/displayed-value lineage  50% → 100%
Q4 bounded evidence trace          90% → 100%
```

Phase 2 source cross-check matched the workspace for the bounded email displayed-value path. Fix #3 is therefore PASS / COMPLETE for that bounded identity-qualified path. This does not claim unrelated Agentic gaps such as `UpdatedAt` are solved.

## Level 2 — Agentic + Jin12 + Kesetovic

Benchmark wrapper:

```text
branch  benchmark/product-value-level2-79b0
commit  749e24406d050a2b738751e086f02320cd28cf86
run     35900111059 — PASS, 3 / 3
```

Pinned targets:

```text
Agentic   hackersandwizards/agentic-engineering-training-angular@22f2aab64617f4de7984370a5bd40e8c9535dbf5
Jin12     jin12-xyz/CRM@00493af54d4d9e146d1c6eb75f5dc8f3898f09ec
Kesetovic kesetovic/crm-system@8e3b74bec4fdcd0144bd65f0c1b49c8e801bd2f7
```

Workspace-only artifacts:

```text
Agentic   10768851896  sha256:337fa5ebc2caf272e7c51537fa07508f3539da27a76445fc0ee837f3c5e6e75c
Jin12     10767973149  sha256:480ba229f9648a4f39cce90d285f7285f416ca4f6314fd0d019b75d787d21cd9
Kesetovic 10768986941  sha256:cadf9c0c0e92641318f81a11450f2a1f2bba2c60348b6abcece71ed3700a7e3d
```

All three pinned target builds succeeded in the Level-2 run.

Phase 1 answers were frozen from workspace-only evidence before source cross-check. Phase 2 then inspected only the minimum pinned source needed to verify the selected known answers.

### Selected-probe outcome

Defensible selected-probe scoring after source cross-check:

```text
Agentic Users Update     ~94.5%
Jin12 Contacts Update    100.0%
Kesetovic PackOrder       82.5%
selected-probe aggregate ~91.6%
backend PO/QC core       ~95.3%
```

These percentages are diagnostic measurements, not a formal acceptance threshold.

### What is now strong

- Agentic Users Update: endpoint/capability, permissions, update preconditions, UI→PATCH linkage and bounded displayed-email origin are all reconstructable.
- Jin12 Contacts Update: controller/interface → exact direct DI registration → concrete `ContactService.UpdateAsync`, existence guard, seven field updates and persistence are reconstructable.
- Kesetovic PackOrder: Packer/Admin permissions, order existence, NEW-only precondition, transition to PACKED, child UI action → parent handler → service → PUT endpoint are reconstructable.

### Remaining independent product-value gaps

The broad product-value gate remains NOT PASS because the corpus still exposes independent gaps:

1. construction/default/computation state;
2. semantic integration side effects such as SignalR `OrderSignal`;
3. feature-summary preservation of grounded workflow rules;
4. positive unchanged-real-project R7.14 cross-layer yield.

Kesetovic also still lacks a proven displayed-value origin for the rendered order-card fields; this is separate from the already repaired action/API path.

## Fix #4 prework candidate — observable construction state

Because V0.4.7-E is still formally locked, this work is retained as a validated future candidate only and is not merged into `main`.

Implementation branch:

```text
fix/product-value-construction-state
```

Implementation commit:

```text
35c8e5c5f856e15568aa963bb2d76268008c5570
fix: prove observable constructed state
```

Production delta is exactly:

```text
src/Pkc.CSharp/CSharpEvidenceScanner.cs
src/Pkc.CSharp/CSharpObservableConstructionEnricher.cs
tests/Pkc.CSharp.Tests/ObservableConstructionStateRegressionTests.cs
```

Bounded proof rule:

A local object initializer may become authoritative constructed-state evidence only when exact project-semantic local-symbol identity proves both:

1. the same whole object is passed to a downstream invocation outside the return expression; and
2. the same whole object is passed to an invocation on the return path.

The repair does not globally promote initializer assignments and does not claim persistence merely from construction.

Negative regressions keep return-only constructions and downstream calls involving a different local fail-closed.

### Deterministic validation

First validation wrapper run:

```text
run 35901814917
```

Candidate evidence before the external target-build issue:

```text
Release build        PASS, 0 warnings / 0 errors
focused regressions  8 / 8 PASS
C# full suite         282 / 282 PASS
frontend full suite    23 / 23 PASS
```

That run later stopped at the pinned Kesetovic build because NuGet temporarily treated AutoMapper 12.0.1 advisory `GHSA-rvv3-g6hj-g44x` as NU1903 Warning-As-Error. PKC generation had not started; this was target/environment evidence, not a candidate regression.

A wrapper-only retry retained the unchanged implementation and treated target build outcome independently, matching the real-repository benchmark protocol:

```text
wrapper commit 3529c3a87597cb58ac0cd7f317c39d6311bddff3
run            35902984101 — PASS
```

On the retry the pinned target build also succeeded. Real-repository generation then proved the generated Kesetovic `AddOrder` workflow contains:

```text
Sets `newOrder.OrderStatus` to `OrderStatus.NEW`.
Sets `newOrder.BonusAwarded` to `orderDto.OrderPrice * 0.05`.
Sets `newOrder.CustomerName` to `orderDto.CustomerName`.
```

Fix #4 is therefore technically validated as a future product-value candidate, but it is NOT production and MUST NOT be merged until the formal D gate unlocks E.

## Formal gate remains unchanged

Formal independent-review target:

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

Required request:

`docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-17-request.md`

Current formal state:

```text
R7.10       REPAIRED / ALL GATES PASS / PENDING INDEPENDENT REREVIEW #17
V0.4.7-D    PENDING INDEPENDENT REREVIEW #17
V0.4.7-E    LOCKED
R7.14       NOT PASS / REQUIRED FOR E
V0.5        LOCKED
```

The next formal action is an independent rereview of exact `96205a9...`. A product-value implementation thread must not self-certify that gate.
