# Loren blind knowledge-only review — 2026-09-12

## Purpose

This records the first-pass answers produced **without opening the Loren source repository or `.pkc` raw evidence**.

The reviewer received only the generated single-file handoff:

```text
PKC_KNOWLEDGE.md
```

PKC compiler commit used for the artifact:

```text
326f556517c27085ed027eecdeef7e4125c851a6
```

Pinned Loren benchmark commit:

```text
e9e81651d380d7d40998f235cfdc7f119fe67af8
```

The answers below are intentionally frozen before source cross-check. Any correction required after reopening source is a benchmark finding, not an edit to the blind answer.

## Blind answers

### 1. What observable capabilities does Loren expose?

Observed capability areas in the pack:

- owner authentication: login, logout and authenticated session status;
- project discovery/listing and project bootstrap;
- assistant/agent run execution;
- action-proposal approval and cancellation;
- login-page/root/session/health HTTP surfaces;
- a conditional development-only run endpoint.

The pack does not establish approved product intent or delivery history; these are code-observed capabilities only.

**Blind confidence: high.**

### 2. How does owner authentication work?

`POST /auth/login` checks whether owner authentication is configured. If not configured it returns a 503 problem and specifically points to `LOREN_OWNER_PASSWORD`. An invalid password returns unauthorized. A successful login signs in the current HTTP context through ASP.NET authentication and returns `authenticated = true`.

`POST /auth/logout` requires authorization, signs out the current HTTP context and returns `authenticated = false`.

`GET /api/session` requires authorization and reports an authenticated owner as `Owner`.

`GET /login` redirects an already-authenticated user to `/`; otherwise it returns the login HTML.

**Blind confidence: high.**

### 3. What does the main run operation do at a product/system level?

`POST /api/run` is an authorized assistant/agent execution entry point. It rejects an empty message and requires an owner identity. The compiled workflow indicates that a run:

1. builds project context;
2. incorporates available memory context;
3. creates brain context from the user request;
4. runs an agent loop that calls the brain;
5. stops on final brain output or processes an action request;
6. executes actions through `IActionGateway`;
7. enforces a maximum action count;
8. can collect action proposal IDs during the run;
9. returns the run result.

Unknown explicit project aliases produce not-found behavior; argument errors map to bad request.

A second `POST /internal/dev/run` surface exists only when `developmentRunEndpointEnabled` is true.

**Blind confidence: medium-high.**

Reason confidence is not high: the feature-level `Run Operations` page does not explicitly summarize project/memory/agent stages; those stages have to be reconstructed from workflow backend-flow and evidence names.

### 4. How are project context and memory involved in a run?

The run workflow shows `LorenProjectContextBuilder.BuildAsync` being called by `LorenRunService.RunAsync`.

The project-context builder can:

- find a project by explicit alias;
- infer a project from the user message by listing known projects and matching project/repository cues;
- reject an explicitly supplied unknown alias;
- prepare conversation/history context;
- call `LorenMemoryContextBuilder.BuildAsync`.

The memory-context builder calls `IMemoryStore.ListCurrentForProjectAsync`, selects a bounded set of memory records, builds system context and contributes that context to the run.

The project builder then contributes project context and memory system context before the agent loop executes.

**Blind confidence: medium.**

This answer is available from the pack, but mostly by interpreting detailed backend-flow method names and conditions rather than a direct product-level explanation.

### 5. How are projects listed and bootstrapped, and what can fail?

`GET /api/projects` requires authorization and returns the project catalog/list.

`POST /api/projects/bootstrap` also requires authorization. The underlying bootstrap service checks for an existing alias and refuses to rebind an alias that already exists. It creates project/repository IDs and saves the new catalog entry.

Observed HTTP outcomes include:

- successful bootstrap → OK;
- argument error → bad request;
- existing/rebinding conflict represented by `InvalidOperationException` → conflict.

**Blind confidence: high.**

### 6. What are action proposals and what happens when they are approved or cancelled?

The pack strongly suggests that the currently observed proposal type is a GitHub create-branch proposal.

Approve flow:

- requires authorization and owner identity;
- validates the proposal identifier;
- loads the proposal;
- verifies proposal owner identity;
- verifies the current project/repository still matches the proposal repository locator;
- recomputes a create-branch action-intent fingerprint and verifies it still matches the proposal;
- requires the proposal-store approval decision to be approved;
- executes the resulting action through `IActionGateway.ExecuteAsync`;
- maps unknown / owner mismatch / approved / other statuses to not-found / forbid / OK / conflict.

Cancel flow:

- requires authorization and owner identity;
- calls `ICreateBranchProposalStore.CancelAsync`;
- maps unknown / owner mismatch / cancelled / other statuses to not-found / forbid / OK / conflict.

**Blind confidence: medium-high.**

Reason confidence is not high: the feature is named generically `ActionProposals Status Management`, but the actual proposal purpose has to be inferred from implementation names such as `CreateBranchProposal`, `GitHubActions.CreateBranch` and `ApproveProposalAndCreateBranchAsync`. The pack does not provide a concise product-level definition of an action proposal.

### 7. Which behavior is conditional or development-only?

`POST /internal/dev/run` is registered only when `developmentRunEndpointEnabled` is true.

The pack explicitly calls this out in both the index and Run feature.

**Blind confidence: high.**

### 8. What important permissions, validations and failure paths exist?

Important observed examples:

- protected project/run/proposal/session/logout routes require authorization;
- run requires a non-empty message and owner identity;
- an explicitly supplied unknown project alias can produce not found;
- argument errors in run/bootstrap become bad request;
- bootstrap refuses an existing alias/rebinding attempt and maps it to conflict;
- login returns 503 when owner auth is not configured and unauthorized for an invalid password;
- action proposal approve/cancel protect owner identity and map proposal/result status to not-found, forbid, OK or conflict;
- the agent loop fails if the brain returns neither final output nor an action request;
- the agent loop enforces a configured maximum action count.

**Blind confidence: high for the listed behavior.**

### 9. What important side effects or integrations are visible?

Explicit side effects visible at product level include ASP.NET sign-in and sign-out.

The deeper workflow flow also shows integrations with:

- the project catalog;
- the memory store;
- the brain (`IBrain.ThinkAsync`);
- the action gateway (`IActionGateway.ExecuteAsync`);
- the create-branch proposal store;
- an audit sink;
- GitHub/create-branch-oriented action intent/proposal logic.

**Blind confidence: medium.**

The pack exposes these integrations mostly through backend-flow method names. Several workflow `Side effects` sections still say no grounded information even though the flow makes externally meaningful integration calls visible.

### 10. What does PKC explicitly not know yet?

The pack repeatedly states that it does not yet have:

- Azure DevOps delivery history or product intent;
- frontend/UI interaction paths for these Loren workflows;
- runtime UI confirmation;
- business approval/intent beyond the observed code.

It explicitly instructs the reader not to promote `code-observed` behavior into approved business truth.

**Blind confidence: high.**

## Blind-pass findings before reopening source

### Finding A — Run product abstraction is still too implicit

Classification: `missing important behavior` / `comprehension-breaking noise`.

The pack contains enough raw workflow evidence to reconstruct the run pipeline, but `Run Operations` does not directly explain the important stages:

```text
request
→ project resolution/context
→ project memory context
→ brain/agent loop
→ action execution / proposal collection
→ result
```

A PO-level question about how a run works should not require interpreting dozens of implementation method names.

### Finding B — Action proposal purpose is under-explained

Classification: `missing important behavior`.

The pack gives detailed approve/cancel mechanics but does not give a concise product/system definition of the proposal being acted upon. The current reader has to infer create-branch semantics from class/method names.

### Finding C — Workflow detail remains noisy below the feature layer

Classification: `comprehension-breaking noise` (non-blocking until source cross-check decides impact).

The Run workflow still contains helper mechanics such as string truncation, character normalization and collection iteration. Feature-level filtering improved significantly, but a detailed reader can still hit implementation noise before the important application flow.

### Finding D — Integration/side-effect presentation is incomplete

Classification: `missing important behavior` candidate.

The run and action-proposal workflows expose important calls to project catalog, memory store, brain, action gateway, proposal store and audit sink in `Backend flow`, while `Side effects` may remain empty. The source cross-check must determine which of these should be promoted as grounded integrations/side effects rather than left as raw call graph.

## Next step

Reopen the pinned Loren source only after this file is committed, compare every answer/finding against implementation, and classify any correction needed as a V0.4.4 benchmark finding.
