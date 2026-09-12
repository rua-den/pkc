# Loren blind knowledge-only review — 2026-09-12

## Purpose

This benchmark records whether an AI can understand Loren from PKC's portable knowledge **without opening the Loren source repository or `.pkc` raw evidence**.

Pinned Loren benchmark commit:

```text
e9e81651d380d7d40998f235cfdc7f119fe67af8
```

The review is intentionally staged:

```text
PKC_KNOWLEDGE.md only
→ freeze answers/findings
→ reopen source and classify gaps
→ fix only proven generic compiler gaps
→ regenerate
→ blind-read again
→ verify structured-pack parity
```

## First blind pass

PKC compiler commit used for the original artifact:

```text
326f556517c27085ed027eecdeef7e4125c851a6
```

### What the first pass could answer

The knowledge-only reader could correctly identify:

- owner authentication and session behavior;
- project list/bootstrap behavior;
- the authorized `/api/run` surface and the conditional `/internal/dev/run` surface;
- action-proposal approve/cancel status behavior;
- important failure mappings and unknown boundaries.

The main run flow and proposal meaning were technically recoverable, but only by reading detailed backend-flow method names.

### First-pass blockers

#### Finding A — Run product abstraction was too implicit

Classification: `missing important behavior` / `comprehension-breaking noise`.

The pack contained enough detail to reconstruct:

```text
request
→ project resolution/context
→ project memory context
→ brain/agent loop
→ action execution / proposal collection
→ result
```

but `Run Operations` did not expose that application-level collaboration directly.

#### Finding B — Action proposal purpose was under-explained

Classification: `missing important behavior`.

Approve/cancel mechanics were detailed, but a reader had to infer that the currently observed proposal is a GitHub create-branch proposal from names such as `CreateBranchProposal`, `GitHubActions.CreateBranch`, and `ApproveProposalAndCreateBranchAsync`.

#### Finding C — Workflow detail remained noisy

Classification: `comprehension-breaking noise`, non-blocking at feature-first navigation level.

Detailed workflow files still include implementation mechanics that are useful for traceability but not suitable for product-level orientation.

#### Finding D — Integration presentation was incomplete

Classification: `missing important behavior` candidate.

Important application collaborations such as project catalog, memory store, brain, action gateway, proposal store and audit sink were visible only in detailed backend flow.

## Source cross-check after the first pass

The pinned Loren source confirmed that Findings A and B were real product-semantic gaps rather than wording preferences:

- `LorenRunService.RunAsync` prepares project context, invokes the project-context builder, participates in proposal collection, invokes the agent loop and returns run/audit/proposal results;
- `LorenProjectContextBuilder.BuildAsync` resolves/infer projects and calls `LorenMemoryContextBuilder.BuildAsync` when project context exists;
- `LorenMemoryContextBuilder.BuildAsync` loads current project memories, filters/bounds them, and produces system context;
- the currently observed action-proposal approval path is specifically create-branch oriented and revalidates canonical target/fingerprint before execution through `IActionGateway`;
- cancel records the cancellation without executing the GitHub change.

The fix therefore had to be generic application-flow promotion, not Loren-specific prose.

## Generic fix

PKC now promotes a selective **Observed capability flow** from workflow call evidence into product feature pages.

The promotion deliberately:

- keeps endpoint → application-service edges;
- keeps cross-component/capability-boundary collaboration;
- prioritizes service/gateway/store/brain/loop/catalog/builder boundaries;
- preserves important interface calls such as `IBrain`, `IActionGateway`, `IMemoryStore`, `IProjectCatalog`;
- removes obvious plumbing such as `ToString`, `Parse`, `TryParse`, `New`, `Append`, equality/hash helpers and same-owner helper calls;
- caps feature-level flow so the full call graph remains in workflow detail instead of flooding the feature page.

Regression coverage is in:

```text
tests/Pkc.CSharp.Tests/ProductFeatureCapabilityFlowTests.cs
```

## Second blind pass

PKC compiler commit:

```text
4f7f75e76a1f158a880e8f2d1d64ea0bea0d36e7
```

Pinned Loren workflow:

```text
loren-external-trial #104 — PASS
```

The second pass again read **only `PKC_KNOWLEDGE.md`** before consulting source.

### Run comprehension — PASS

From the feature-level `Run Operations` page alone, the reader can now identify this grounded application flow:

```text
POST /api/run
→ LorenRunService.RunAsync
→ LorenProjectContextBuilder.BuildAsync
→ LorenMemoryContextBuilder.BuildAsync
→ IMemoryStore.ListCurrentForProjectAsync

LorenRunService.RunAsync
→ AgentLoop.RunAsync
→ IBrain.ThinkAsync
→ IActionGateway.ExecuteAsync
```

The same feature also exposes project-catalog resolution/inference, proposal collection/store participation, audit collection, the message/owner validation paths, agent-loop limits, and the conditional development endpoint.

The reader no longer needs to descend into the workflow evidence section merely to discover that project + memory context feed the agent run.

**Blind result: PASS.**

### Action-proposal comprehension — PASS

The feature-level `ActionProposals Status Management` page now exposes:

```text
approve endpoint
→ ApproveProposalAndCreateBranchAsync
→ ICreateBranchProposalStore.GetAsync / ApproveAsync
→ IProjectCatalog.GetAsync
→ ActionIntentFingerprint.Compute
→ IActionGateway.ExecuteAsync

cancel endpoint
→ CancelProposalAsync
→ ICreateBranchProposalStore.CancelAsync
```

This is enough for a source-blind reader to identify the currently observed proposal as a create-branch proposal, understand that approval revalidates canonical project/intent state before execution, and distinguish cancellation from execution.

**Blind result: PASS.**

### Finding C reassessment

Detailed workflows still contain low-level implementation evidence. This remains acceptable because:

- the receiving AI is instructed to start from index → feature;
- feature pages now expose the high-signal application collaboration;
- workflow detail remains available for exact traceability and failure semantics;
- removing the detailed evidence would reduce auditability.

**Result: non-blocking boundary, keep as-is unless a future blind review proves comprehension harm.**

### Finding D reassessment

Not every cross-component call should be labeled a side effect. The new `Observed capability flow` is a more accurate place for project catalog, memory store, brain, action gateway and proposal-store collaborations.

Explicit side effects such as authentication sign-in/sign-out remain in the side-effect section; application collaborations remain in capability flow.

**Result: resolved by better abstraction, not by broad side-effect promotion.**

## Structured-pack / single-file parity

The artifact generated by commit `4f7f75e...` contains 23 Markdown files under `knowledge/`.

Parity verification result:

```text
23 / 23 structured Markdown files are embedded verbatim in PKC_KNOWLEDGE.md
missing markers: 0
missing content: 0
```

The generated `PKC_KNOWLEDGE.zip` contains exactly the same 23 `knowledge/*.md` files and contains no raw `.pkc` data or source files (`.cs`, `.ts`).

**Handoff parity result: PASS.**

## Current V0.4.4 acceptance position

At commit:

```text
4f7f75e76a1f158a880e8f2d1d64ea0bea0d36e7
```

verified gates are:

```text
core/unit/tool                               PASS
PokeTrade known-answer regression            PASS
Loren pinned real-project compile            PASS
Loren-main canary                            PASS
UI/backend validation known-answer contract  PASS
Loren blind knowledge-only comprehension     PASS
Loren structured/single-file handoff parity  PASS
```

V0.4.4 is **ready for independent external review**, but is not self-declared complete. The next acceptance action is reviewer assessment of this checkpoint. Only after that review passes should work move to the V0.4.5 second independent real-repository generalization gate.
