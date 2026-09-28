# RD8-B Deferred Command-Queue Linking Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Link a deferred command-queue producer to its concrete handler only when the scanned C# solution proves the exact queued identity and dispatch path.

**Architecture:** Extend `CSharpProjectSemanticEnricher` to correlate the producer's persisted handler identity with dispatcher resolution and one unique callable handler. Propagate the resulting deferred relation through `FeatureCandidateBuilder` so the handler fact and relation survive into the product-knowledge candidate graph. Render the relation separately from synchronous calls in `GroundedKnowledgeSynthesizer`; leave the path unresolved when any identity hop is missing or ambiguous.

**Tech Stack:** .NET, Roslyn, xUnit, existing PKC evidence facts/relations and knowledge renderers.

**Spec:** `docs/plans/2026-09-28-rd8-repair-b-deferred-command-queue-spec.md`

## Global Constraints

- Use exact semantic identity; never use simple-name similarity, suffixes, folder proximity, or unrelated queue operations as proof.
- A deferred relation describes a statically observed route and never claims runtime execution, successful delivery, timing, retries, schedules, or business intent.
- Preserve existing synchronous `invokes`, DI `dispatches`, sole-implementation dispatch, and fail-closed behavior.
- Do not combine recurring-job triggers (repair (c)) with queue producer-to-handler linking (repair (b)).
- Use invented names in tests and sanitized behavior-only evidence in PKC docs; never copy private source or raw facts.
- Follow repository scope: regression first; no unrelated refactor; one coherent commit and push after local verification and review.

---

### Task 1: Freeze the sanitized source shape

**Files:**
- Read only in the approved source-enabled environment; no target files are changed.
- Record the sanitized structural result in `docs/reviews/2026-09-28-rd8-private-queue-shape.md` only after review.

**Interfaces:**
- Consumes: the approved read-only target checkout and the existing sanitized RD8-B queue finding.
- Produces: a behavior-only description of the producer identity expression, persisted field identity, dispatcher resolution family, handler-entry selection, and whether resolution is unique. Exclude target names, paths, code text, and raw evidence.

- [ ] **Step 1: Trace the producer-to-handler identity chain read-only.** Confirm the queue writer's identity value, the same persisted member as read by the dispatcher, the resolution mechanism, and the callable handler reached. Do not inspect unrelated workflow behavior.
- [ ] **Step 2: Record only the sanitized shape.** State the syntax/mechanism family and uniqueness conditions without source identifiers or code. If the identity cannot be followed end to end, stop before implementation and record exactly which hop remains unproven.

### Task 2: Add a positive red regression

**Files:**
- Create: `tests/Pkc.CSharp.Tests/DeferredCommandQueueDispatchRegressionTests.cs`
- Read: `src/Pkc.CSharp/CSharpProjectSemanticEnricher.cs`
- Read: `src/Pkc.Knowledge/FeatureCandidateBuilder.cs`
- Read: `src/Pkc.Knowledge/GroundedKnowledgeSynthesizer.cs`

**Interfaces:**
- Consumes: the frozen sanitized shape from Task 1 and the existing `FactDocument`/`EvidenceRelation` test patterns.
- Produces: a regression asserting one producer-to-handler `deferred-dispatch` relation for one exact, unique identity chain.

- [ ] **Step 1: Create an invented C# project fixture.** Give the producer, queued record, dispatcher, interface, and handler invented names. Model only the identity and resolution mechanisms confirmed in Task 1.
- [ ] **Step 2: Scan the fixture using the existing project-aware scanner.** Assert the producer and handler facts exist, then assert the exact relation kind, producer fact ID, handler fact ID, and dispatcher evidence location.
- [ ] **Step 3: Run the focused test against the current implementation.** Run `dotnet test tests/Pkc.CSharp.Tests/Pkc.CSharp.Tests.csproj --filter FullyQualifiedName~DeferredCommandQueueDispatchRegressionTests`; verify it fails because the deferred edge is absent, not because the fixture fails to compile.

### Task 3: Add fail-closed red regressions

**Files:**
- Modify: `tests/Pkc.CSharp.Tests/DeferredCommandQueueDispatchRegressionTests.cs`

**Interfaces:**
- Consumes: the positive fixture contract from Task 2.
- Produces: tests proving incomplete or ambiguous identity chains do not create a deferred edge.

- [ ] **Step 1: Add a mismatched-identity fixture.** The producer writes one identity and the dispatcher resolves a different queued member/value; assert no `deferred-dispatch` relation.
- [ ] **Step 2: Add an ambiguous-handler fixture.** Two concrete candidates share the supported identity shape; assert no guessed handler edge and assert the unresolved relation is retained when producer identity is known.
- [ ] **Step 3: Add a missing-consumer fixture.** The producer queues a handler identity but the scanned solution has no proven dispatcher resolution; assert no producer-to-handler edge and preserve an unresolved unknown.
- [ ] **Step 4: Run the focused tests.** Confirm all negative tests pass on the current implementation, establishing that they guard false-link behavior before production code is added.

### Task 4: Implement exact deferred-dispatch correlation and candidate propagation

**Files:**
- Modify: `src/Pkc.CSharp/CSharpProjectSemanticEnricher.cs`
- Modify: `src/Pkc.Knowledge/FeatureCandidateBuilder.cs`
- Modify: `tests/Pkc.CSharp.Tests/DeferredCommandQueueDispatchRegressionTests.cs`

**Interfaces:**
- Consumes: compiler symbols, queued identity facts from Task 1, callable facts already indexed by `CSharpProjectSemanticEnricher`, and the existing feature-candidate traversal model.
- Produces: `deferred-dispatch` from the producer fact ID to one unique handler fact ID and carries that relation plus handler fact into the feature candidate graph; otherwise carries `unresolved-deferred-dispatch` only when the producer identity itself is grounded.

- [ ] **Step 1: Add the regression-backed relation constants.** Define `DeferredDispatchRelation = "deferred-dispatch"` and `UnresolvedDeferredDispatchRelation = "unresolved-deferred-dispatch"` beside the existing dispatch relation constants.
- [ ] **Step 2: Correlate only the confirmed chain.** Resolve producer identity, persisted member read, dispatcher resolution, and callable handler using exact Roslyn/project identity. Require exactly one producer/handler match and retain the dispatcher call as the relation source location.
- [ ] **Step 3: Fail closed at every incomplete hop.** Do not emit a handler relation for missing, mismatched, unsupported, or ambiguous paths. Emit unresolved evidence only where a queue producer identity is proven.
- [ ] **Step 4: Propagate the new relation kinds through `FeatureCandidateBuilder`.** Treat `deferred-dispatch` as a traversable dispatch edge so its uniquely proven handler fact is included and can continue bounded call traversal. Retain `unresolved-deferred-dispatch` as non-traversing uncertainty evidence. Do not broaden synchronous `invokes` or DI-dispatch authority.
- [ ] **Step 5: Assert candidate transport explicitly.** Build the feature candidate from the positive fixture and assert the `deferred-dispatch` relation and concrete handler fact are present. For unresolved fixtures, assert the unresolved relation survives without a guessed handler fact/edge.
- [ ] **Step 6: Run the focused regression suite.** Confirm the positive test changes from red to green and all negative tests remain green.

### Task 5: Render deferred flow distinctly

**Files:**
- Modify: `src/Pkc.Knowledge/GroundedKnowledgeSynthesizer.cs`
- Modify: `tests/Pkc.CSharp.Tests/DeferredCommandQueueDispatchRegressionTests.cs`

**Interfaces:**
- Consumes: the two relation kinds transported by `FeatureCandidateBuilder` and existing feature-flow facts.
- Produces: a readable deferred queue route and an honest unresolved-dispatch unknown without changing synchronous flow wording.

- [ ] **Step 1: Add deferred flow rendering.** Render `deferred-dispatch` as a queued/deferred route; do not phrase it as a direct `invokes` edge or as proof the handler ran.
- [ ] **Step 2: Add unresolved rendering.** Explain that the queued handler could not be proven when `unresolved-deferred-dispatch` is present; never invent a concrete handler.
- [ ] **Step 3: Assert output wording and downstream product flow.** Verify the positive fixture renders a deferred route, not a synchronous call, and the ambiguous fixture reports uncertainty without a handler claim. Also assert `ProductFeatureBuilder` receives a usable source→handler edge rather than silently dropping the deferred route.

### Task 6: Verify and hand off the checkpoint

**Files:**
- Modify: `docs/status.md`
- Modify: `docs/handoff.md`
- Update: `docs/reviews/2026-09-28-rd8-private-reprobe-phase1.md` only with approved sanitized result/score.

**Interfaces:**
- Consumes: the completed regression, implementation diff, and focused/full verification output.
- Produces: a reviewed main-branch checkpoint with exact SHA, tests/build results, sanitized product result, and remaining RD8-B status.

- [ ] **Step 1: Run related semantic regressions.** Run `dotnet test tests/Pkc.CSharp.Tests/Pkc.CSharp.Tests.csproj --filter "FullyQualifiedName~CSharpProjectSemanticEnricher|FullyQualifiedName~DirectDiDispatch|FullyQualifiedName~FeatureCandidateBuilder"` and resolve any regression.
- [ ] **Step 2: Run repository gates.** Run `dotnet test PKC.sln` followed by `dotnet build PKC.sln --configuration Release`; record the exact exit codes and totals.
- [ ] **Step 3: Review the complete diff.** Confirm no target-specific names, source bodies, raw facts, speculative schedule behavior, or unrelated refactors appear.
- [ ] **Step 4: Update status and handoff.** Keep RD8-B NOT PASS until the remaining probe questions score; keep repair (c), E1, and later checkpoints locked.
- [ ] **Step 5: Commit and push once.** Use one coherent repair-(b) commit on `main` only after local gates and review pass.
