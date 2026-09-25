# Spec — Conditional and Mutually Exclusive State Effects

Date: 2026-09-25
Checkpoint: V0.4.7-E0 / RD8-B repair (a)
Status: SPEC / IMPLEMENTATION AUTHORIZED (regression-first) / not started

Origin: RD8-B probe #1 calibration blocker, recorded in `docs/reviews/2026-09-25-rd8-private-validation-result.md`.

The implementer does **not** need, and must not request, private-target source. The synthetic shape below is sufficient.

## Defect

A command method sets state inside an `if / else`. The two arms are mutually exclusive:

```text
if (<predicate>)            // e.g. "no open dependent records"
{
    entity.A = false;
}
else
{
    entity.B = true;
    entity.C = false;
    entity.D = false;
}
```

The generated workflow renders all four assignments under `## State changes` as one flat, unconditional list:

```text
- Sets `entity.A` to `false`.
- Sets `entity.B` to `true`.
- Sets `entity.C` to `false`.
- Sets `entity.D` to `false`.
```

A PO/QA agent then states as **proven** that one operation sets A, B, C and D together. That is false: only one arm runs. Under the benchmark protocol a wrong proven claim is a blocker.

The same flattening also drops conditions for a plain `if` with no `else`. The mutation is rendered as if it always happens.

## Current code (verified)

- Mutation facts are created per assignment, with metadata `target`, `value`, `operator`, `targetSymbolKind`, `stateMutationCandidate` and `targetSymbol`. They carry no enclosing-branch information (`src/Pkc.CSharp/CSharpRepositoryScanner.cs:270-286`, unary forms at `:288-318`).
- `CSharpMutationContextEnricher` already re-parses each mutation's syntax tree and reclassifies initializer assignments (`src/Pkc.CSharp/CSharpMutationContextEnricher.cs:20-60`). This is the natural place to attach branch context.
- Rendering: `GroundedKnowledgeSynthesizer` projects mutations through `DescribeMutation` into flat sentences (`src/Pkc.Knowledge/GroundedKnowledgeSynthesizer.cs:494-533`). `EvidenceAwareKnowledgeSynthesizer` passes `StateChanges` through (`src/Pkc.Knowledge/EvidenceAwareKnowledgeSynthesizer.cs:68`, `:126`). `MarkdownKnowledgeRenderer` prints the section (`src/Pkc.Knowledge/MarkdownKnowledgeRenderer.cs:52`).
- Existing state-change tests to keep green: `CSharpInitializerMutationTests`, `PokeTradeKnowledgeSemanticsTests`, `ValueLineageComputationCausalityRegressionTests`, `TransitiveMutationCausalityRegressionTests` (all in `tests/Pkc.CSharp.Tests`).

## Required behaviour

### E1 — capture branch context on each mutation

For each `mutation` fact, record its enclosing branch path inside the owning method, from outermost to innermost. Each step is:

- a stable branch-construct id (for example the construct's source span);
- the arm (`then` / `else` / `else-if#n` / `case:<label>` / `default`);
- the arm's condition text when it is a simple expression. Leave the condition text null when it is not renderable.

Cover `if`/`else`, `else if` chains and `switch` statement sections. Conditional expressions (`?:`) and `switch` expressions that produce the assigned *value* are out of scope. They are values, not branches of the assignment.

Proposed metadata keys (do not exist today): `branchPath` (serialized deterministically) and `branchConditional` = `true|false`.

A mutation with an empty path is unconditional, which is the current behaviour.

### E2 — render conditional and exclusive effects honestly

In `## State changes`:

- unconditional mutations render exactly as today (byte-identical output for existing fixtures);
- mutations under one arm are grouped under that arm, for example:

  ```text
  - When `<predicate>`:
    - Sets `entity.A` to `false`.
  - Otherwise:
    - Sets `entity.B` to `true`.
    - ...
  ```

- sibling arms of the same construct must be labelled as **alternatives**. They must never be merged into one list;
- a plain `if` with no `else` renders `When <cond>:` with no `Otherwise`;
- nested branches nest, with bounded depth. Past the bound, render `Under additional conditions:` rather than dropping the condition;
- a condition that is not renderable renders `In one branch (condition not shown):` / `In an alternative branch:`. The exclusivity statement is always kept.

The same grouping must reach every consumer that lists state changes, including product-feature output if it aggregates `StateChanges`. Grounding counts (`grounding.withStateChanges`) keep counting the workflow once.

### E3 — no new authority

Do not infer which arm is "normal", and do not evaluate predicates. Do not drop mutations. Do not reorder unconditional mutations relative to today.

## Required regressions

Add a focused test class, for example `tests/Pkc.CSharp.Tests/ConditionalStateEffectRegressionTests.cs`, with synthetic fixtures (invented names).

Write the tests red first and confirm current main fails for the intended reason: flat merged output.

Positive:

1. `if/else` exact defect shape → two alternative groups; A is never listed together with B/C/D as unconditional.
2. `if` without `else` → one `When` group plus the unconditional siblings unchanged.
3. `else if` chain with three arms → three alternative groups.
4. `switch` statement with two cases and a default → three alternative groups.
5. Nested `if` inside an arm → nested grouping.
6. Unconditional-only method → byte-identical to current output.
7. Non-renderable condition → exclusivity wording preserved.

Negative / stability:

- mutations inside lambdas or local functions keep today's handling, with no branch path invented from the outer method;
- initializer assignments stay reclassified (existing test green);
- deterministic output across file creation order.

## Verification

```text
focused:  dotnet test tests/Pkc.CSharp.Tests --filter "FullyQualifiedName~ConditionalStateEffect"
related:  state-change / mutation / knowledge-semantics test classes listed above
full:     dotnet test PKC.sln + dotnet build PKC.sln -c Release
one coherent implementation commit on a topic branch; do not push main
```

## After it lands (approved-environment operator)

- Rerun `pkc run` on the disposable copy of the private target.
- Rerun RD8-B probe #1 as phase 1 workspace-only, then phase 2 cross-check.
- Expected: the deactivation effects render as two alternative groups, and the calibration blocker clears.
- Record sanitized scores in `docs/reviews/2026-09-25-rd8-private-validation-result.md`.

Repairs (b) command-queue producer → handler linking and (c) recurring jobs as triggers are **separate** tasks. Do not combine them with this one.
