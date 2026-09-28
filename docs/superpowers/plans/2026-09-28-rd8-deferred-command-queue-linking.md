# RD8-B Deferred Command-Queue Linking Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Link a deferred command-queue producer to its concrete handler only when the scanned C# solution proves the exact queued identity and dispatch path.

**Architecture:** Extend `CSharpProjectSemanticEnricher` to correlate the producer's persisted handler identity with dispatcher resolution and one unique callable handler. Propagate the resulting deferred relation through `FeatureCandidateBuilder`; the normal CLI path reaches that builder through `CrossStackFeatureCandidateBuilder`. Render the relation separately from synchronous calls in `GroundedKnowledgeSynthesizer`; the normal CLI path reaches that synthesizer through `JointVisibilityKnowledgeSynthesizer -> EvidenceAwareKnowledgeSynthesizer -> GroundedKnowledgeSynthesizer`. Preserve a machine-parseable deferred marker through `ProductFeatureBuilder` so product capability flow remains symbol-clean while still visibly labelled deferred.

**Tech Stack:** .NET, Roslyn, xUnit, existing PKC evidence facts/relations and knowledge renderers.

**Spec:** `docs/plans/2026-09-28-rd8-repair-b-deferred-command-queue-spec.md`

## Global Constraints

- Use exact semantic identity; never use simple-name similarity, suffixes, folder proximity, or unrelated queue operations as proof.
- A deferred relation describes a statically observed route and never claims runtime execution, successful delivery, timing, retries, schedules, or business intent.
- Preserve existing synchronous `invokes`, DI `dispatches`, sole-implementation dispatch, and fail-closed behavior.
- Do not combine recurring-job triggers (repair (c)) with queue producer-to-handler linking (repair (b)).
- Use invented names in tests and sanitized behavior-only evidence in PKC docs; never copy private source or raw facts.
- Follow repository scope: regression first; no unrelated refactor; one coherent commit and push after local verification and review.
- `deferred-dispatch` traversal must remain bounded by the existing candidate call-depth policy. Do not change existing synchronous/DI traversal semantics merely to add deferred routing.
- Production-path verification must exercise the same wrappers used by `pkc run`, not only inner helper classes.

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
- [ ] **Step 3: Authorize implementation explicitly.** Change the queue-shape record from `NOT AUTHORIZED` to `AUTHORIZED` only when every identity hop is deterministic and the fixture can reproduce the mechanism family without private identifiers.

### Task 2: Add the positive red regression

**Files:**
- Create: `tests/Pkc.CSharp.Tests/DeferredCommandQueueDispatchRegressionTests.cs`
- Read: `src/Pkc.CSharp/CSharpProjectSemanticEnricher.cs`
- Read: `src/Pkc.Knowledge/FeatureCandidateBuilder.cs`
- Read: `src/Pkc.Knowledge/CrossStackFeatureCandidateBuilder.cs`
- Read: `src/Pkc.Knowledge/GroundedKnowledgeSynthesizer.cs`
- Read: `src/Pkc.Knowledge/EvidenceAwareKnowledgeSynthesizer.cs`
- Read: `src/Pkc.Knowledge/JointVisibilityKnowledgeSynthesizer.cs`
- Read: `src/Pkc.Knowledge/ProductFeatureBuilder.cs`

**Interfaces:**
- Consumes: the frozen sanitized shape from Task 1 and the existing `FactDocument`/`EvidenceRelation` test patterns.
- Produces: a compile-valid regression asserting one producer-to-handler `deferred-dispatch` relation for one exact, unique identity chain.

- [ ] **Step 1: Create an invented C# project fixture.** Give the producer, queued record, dispatcher, resolver abstraction if applicable, and handler invented names. Model only the identity and resolution mechanisms confirmed in Task 1.
- [ ] **Step 2: Scan the fixture using the existing project-aware scanner.** Assert the producer and handler facts exist, then assert the exact relation kind, producer fact ID, handler fact ID, and dispatcher evidence location.
- [ ] **Step 3: Prove the fixture itself is valid.** The fixture must compile/load project semantics successfully; a compilation or MSBuild-load failure is not an acceptable red test.
- [ ] **Step 4: Run the focused test against the current implementation.** Run `dotnet test tests/Pkc.CSharp.Tests/Pkc.CSharp.Tests.csproj --filter FullyQualifiedName~DeferredCommandQueueDispatchRegressionTests`; verify the positive case fails because the deferred edge is absent.

### Task 3: Add fail-closed red regressions

**Files:**
- Modify: `tests/Pkc.CSharp.Tests/DeferredCommandQueueDispatchRegressionTests.cs`

**Interfaces:**
- Consumes: the positive fixture contract from Task 2.
- Produces: tests proving incomplete or ambiguous identity chains never create a concrete deferred edge and, when producer identity itself is proven, become explicit unresolved deferred evidence after implementation.

- [ ] **Step 1: Add a mismatched-identity fixture.** The producer writes one identity and the dispatcher consumes/resolves a different identity path. Assert no concrete `deferred-dispatch`; because producer identity is grounded, also require `unresolved-deferred-dispatch` after implementation.
- [ ] **Step 2: Add an ambiguous-handler fixture.** The supported resolver reaches more than one concrete callable candidate. Assert no guessed concrete handler and require unresolved deferred evidence.
- [ ] **Step 3: Add a missing-consumer fixture.** The producer queues a grounded handler identity but the scanned solution has no proven dispatcher/resolution chain. Assert no concrete handler edge and require unresolved deferred evidence.
- [ ] **Step 4: Add an unsupported-resolver fixture.** Keep the producer identity valid but use a resolver mechanism outside the confirmed supported family. Assert no concrete edge and require unresolved deferred evidence.
- [ ] **Step 5: Add duplicate-callable-fact coverage.** Make the resolver identity deterministic but make final callable-fact selection non-unique. Assert no guessed handler edge and require unresolved deferred evidence.
- [ ] **Step 6: Run the focused suite and classify red correctly.** On current main, the no-concrete-edge assertions may already pass, but every assertion requiring `unresolved-deferred-dispatch` must remain red because that relation kind is not implemented yet. Do **not** misclassify the suite as green merely because false concrete links are absent.

### Task 4: Implement exact deferred-dispatch correlation and candidate propagation

**Files:**
- Modify: `src/Pkc.CSharp/CSharpProjectSemanticEnricher.cs`
- Modify: `src/Pkc.Knowledge/FeatureCandidateBuilder.cs`
- Modify: `tests/Pkc.CSharp.Tests/DeferredCommandQueueDispatchRegressionTests.cs`

**Interfaces:**
- Consumes: compiler symbols, queued identity evidence from Task 1, callable facts already indexed by `CSharpProjectSemanticEnricher`, and the existing feature-candidate traversal model.
- Produces: `deferred-dispatch` from the producer fact ID to one unique handler fact ID and carries that relation plus handler fact into the feature candidate graph; otherwise carries `unresolved-deferred-dispatch` only when the producer identity itself is grounded.

- [ ] **Step 1: Add the regression-backed relation constants.** Define `DeferredDispatchRelation = "deferred-dispatch"` and `UnresolvedDeferredDispatchRelation = "unresolved-deferred-dispatch"` beside the existing dispatch relation constants.
- [ ] **Step 2: Correlate only the confirmed chain.** Resolve producer identity, persisted member read, dispatcher resolution, and callable handler using exact Roslyn/project identity. Require exactly one final callable fact. Retain the dispatcher resolution/call site as the relation source location.
- [ ] **Step 3: Fail closed at every incomplete hop.** Do not emit a handler relation for missing, mismatched, unsupported, or ambiguous paths. Emit unresolved evidence only where a queue producer identity is proven.
- [ ] **Step 4: Propagate the new relation kinds through `FeatureCandidateBuilder`.** Treat `deferred-dispatch` as a traversable dispatch edge so its uniquely proven handler fact is included and can continue bounded traversal. Respect `MaxCallDepth` for this new edge. Retain `unresolved-deferred-dispatch` as non-traversing uncertainty evidence. Do not broaden synchronous `invokes` or existing DI-dispatch authority.
- [ ] **Step 5: Assert direct candidate transport.** Build via `FeatureCandidateBuilder` and assert the positive `deferred-dispatch` relation plus concrete handler fact are present. For unresolved fixtures, assert the unresolved relation survives without a guessed handler fact/edge.
- [ ] **Step 6: Assert normal CLI candidate transport.** Build the same fixture document via `CrossStackFeatureCandidateBuilder` and assert the same deferred relation/handler survive its enrichment/noise filters. This guards the actual `pkc run` path.
- [ ] **Step 7: Run the focused regression suite.** Confirm the positive semantic/candidate tests change from red to green and all fail-closed cases stay concrete-edge-free while unresolved expectations become green.

### Task 5: Render deferred flow distinctly without corrupting product-flow symbols

**Files:**
- Modify: `src/Pkc.Knowledge/GroundedKnowledgeSynthesizer.cs`
- Modify: `src/Pkc.Knowledge/ProductFeatureBuilder.cs`
- Modify: `tests/Pkc.CSharp.Tests/DeferredCommandQueueDispatchRegressionTests.cs`

**Interfaces:**
- Consumes: the two relation kinds transported by `FeatureCandidateBuilder` and existing feature-flow facts.
- Produces: visibly deferred workflow/product flow plus an honest unresolved-deferred unknown, while keeping symbol source/target parsing clean for product-flow ranking.

- [ ] **Step 1: Add deferred workflow-flow rendering.** Render `deferred-dispatch` with the normal Unicode `source → target` separator plus an explicit deferred marker. The wording must say this is a queued/deferred route and that handler execution is not proven; it must not look identical to synchronous `invokes`.
- [ ] **Step 2: Add unresolved rendering.** Add a separate unresolved-deferred unknown explaining that PKC observed a queued handler identity but could not prove one destination. Never invent a concrete handler.
- [ ] **Step 3: Make the deferred marker machine-parseable.** `ProductFeatureBuilder.ParseFlow` currently treats everything to the right of ` → ` as the target symbol. Add a narrowly recognized deferred marker contract so parsing strips the marker before `MethodName`, `SymbolOwner`, component-boundary scoring and graph-distance calculations, then preserves/re-renders the marker in the product capability flow. Do not alter existing plain-flow or DI-flow semantics outside this marker.
- [ ] **Step 4: Assert inner synthesizer output.** Verify `GroundedKnowledgeSynthesizer` labels the positive route deferred and the ambiguous/missing/unsupported cases report uncertainty without a handler claim.
- [ ] **Step 5: Assert normal CLI synthesizer output.** Synthesize through `JointVisibilityKnowledgeSynthesizer` and verify the deferred route and unresolved unknown survive `EvidenceAwareKnowledgeSynthesizer` / visibility wrapping unchanged.
- [ ] **Step 6: Assert product-flow symbol integrity.** Build `ProductFeatureBuilder` output and verify the source/target symbols are clean (no deferred annotation inside either symbol), the edge remains present, and the rendered product capability flow is still visibly labelled deferred.

### Task 6: Verify and hand off the checkpoint

**Files:**
- Modify: `docs/status.md`
- Modify: `docs/handoff.md`
- Update: `docs/reviews/2026-09-28-rd8-private-reprobe-phase1.md` only with approved sanitized result/score after the affected product probes are rerun.

**Interfaces:**
- Consumes: the completed regression, implementation diff, and focused/full verification output.
- Produces: a reviewed main-branch checkpoint with exact production SHA, tests/build results, sanitized product result, and remaining RD8-B status.

- [ ] **Step 1: Run focused/related semantic regressions.** Run the deferred test first, then related `CSharpProjectSemanticEnricher`, direct-DI dispatch, `FeatureCandidateBuilder`, `WorkflowFlowNoise`, product-capability-flow and wrapper-path regressions. Resolve failures locally before broader gates.
- [ ] **Step 2: Run repository gates.** Run `dotnet test PKC.sln` followed by `dotnet build PKC.sln --configuration Release`; record exact exit codes and totals.
- [ ] **Step 3: Review the complete diff.** Confirm no target-specific names, source bodies, raw facts, speculative schedule behavior, or unrelated refactors appear. Confirm recurring-job repair (c) is untouched.
- [ ] **Step 4: Commit and push once.** Use one coherent repair-(b) implementation commit on `main` only after local gates and diff review pass. CI is final verification, not the edit/test loop.
- [ ] **Step 5: Rerun affected private product probes.** Re-probe #7 and #10 workspace-only first, then approved source cross-check. Record only sanitized results.
- [ ] **Step 6: Update status and handoff with the final evidence.** Keep RD8-B NOT PASS unless the selected product-value acceptance rule is actually met; keep repair (c), E1, and later checkpoints locked until explicitly opened.
