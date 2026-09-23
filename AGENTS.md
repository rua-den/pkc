# PKC Agent Instructions

These instructions apply to repository work performed by coding agents and reviewers.
The repository is the source of truth. Milestone/status documents determine current scope and acceptance state.

## Mandatory Bootstrap Read Order

Before changing production code or deciding the next milestone action:

1. inspect `git status`, the current branch, current `main` HEAD, and recent commits;
2. read `docs/status.md`;
3. read `docs/handoff.md`;
4. read `docs/milestones.md`;
5. read `docs/product-knowledge-contract.md`;
6. read the active acceptance plan named by the handoff;
7. read the active review request/result and benchmark evidence named by the handoff;
8. inspect the production code and regression coverage relevant to the current checkpoint.

`docs/status.md` and `docs/handoff.md` own the current milestone state. This file intentionally does not duplicate the current SHA/checkpoint so it does not become stale.

A docs-only `[skip ci]` commit may sit above the exact production SHA under review. When the handoff names an exact production SHA, review that production behavior without resetting `main` to the older SHA.

Do not trust a remembered SHA, chat summary, or older review request over the current repository state.

## Repository Workflow

Before making changes:

1. Read `docs/status.md` and `docs/handoff.md` first.
2. Inspect the current `main` HEAD and recent commits.
3. Understand the current milestone, blockers, acceptance criteria, and existing regression coverage.
4. Do not start the next milestone until the current milestone is explicitly complete.

The milestone rule prevents advancement only; it must never be interpreted as a reason to stop working on the current milestone. If the current milestone is incomplete, continue resolving its remaining blockers until complete or until a genuinely external blocker is proven and documented.

## Independent Review Discipline

When `docs/handoff.md` says the next gate is an **independent review/rereview**, treat that review as the current task before implementation or milestone advancement.

A fresh coding-agent session may perform that independent review, but it must keep the reviewer role clean until the decision is recorded:

- inspect the exact production SHA named by the handoff;
- independently search for a new compile-valid/runtime-valid counterexample;
- do not merely replay already-covered regressions;
- do not modify production while still deciding whether the candidate passes;
- distinguish unsupported shapes that correctly fail closed from real false/over-authoritative output;
- preserve accepted predecessor checkpoints unless a new concrete regression is demonstrated.

If the review **PASSes**:

```text
record independent PASS
→ update status/handoff/acceptance state
→ explicitly close the current checkpoint
→ unlock only the next checkpoint allowed by the acceptance plan
→ then begin that next checkpoint regression-first
```

If the review **FAILs**:

```text
record the exact blocker and violated proof boundary
→ keep later checkpoints locked
→ define the minimum generic regression-first repair
→ only then transition from reviewer to implementation work
→ repair and re-run the required gates
→ require a fresh independent rereview of the repaired production SHA
```

Never let an implementation continuation self-certify its own repaired candidate as the independent review that accepts it.

## Anti-Stall / Forward Progress

Continuously make forward progress while working on PKC.

Unless explicitly asked to stop, continue the current PKC checkpoint autonomously until one of these terminal states is reached:

1. the checkpoint is PASS and locally verified;
2. the checkpoint requires an external review or gate that the agent cannot perform;
3. a genuinely external blocker is proven and documented.

A test failure, build failure, command failure, unclear implementation detail, tool failure, or first unsuccessful approach is **not** a terminal state.

Use this execution loop:

```text
inspect
→ choose the next concrete action
→ execute
→ verify the result
→ recover if blocked
→ continue
```

Do not stop merely because one step failed or produced incomplete information.

### No Passive Waiting

Do not wait passively for:

- a long-running local command;
- a test process that appears hung;
- a build with no useful output;
- GitHub Actions;
- an external service;
- a tool call that is not making progress.

If a process appears stuck:

1. inspect available output;
2. determine whether it is still making useful progress;
3. stop or abandon the process if necessary;
4. isolate the suspected problem with a smaller or focused command;
5. continue from the latest verified state.

Never tell the user to wait for work that can be continued in the current session.

### Bounded Command Execution

Prefer bounded, focused commands over commands that may run indefinitely.

Use progressive verification:

```text
focused regression
→ related test class/project
→ broader relevant suite
→ repository-wide verification when appropriate
```

For potentially long-running commands:

- capture useful output;
- periodically verify that meaningful progress is occurring;
- do not poll indefinitely;
- terminate and investigate when useful progress stops;
- retry only after narrowing scope or changing a material hypothesis.

Do not repeatedly run the same failing or hanging command without new evidence.

### Recovery From Failure

A failure is information, not a stopping condition.

When something fails:

```text
failure
→ inspect error
→ form a concrete hypothesis
→ reproduce narrowly
→ fix or rule out the hypothesis
→ rerun focused verification
→ continue
```

Do not end the task immediately after discovering a failure unless the blocker is genuinely external and cannot be worked around.

If one verification path is unavailable, use another valid local verification path where possible.

### Recovery From Tool Failure

If a repository, shell, API, GitHub, or other tool invocation fails:

1. classify the failure as transient, environmental, permissions-related, or code-related;
2. retry only when there is a concrete reason to expect a different result;
3. otherwise use an alternative inspection or verification method;
4. continue all work that does not depend on the unavailable tool.

Do not loop indefinitely on tool failures.

### Ambiguity Handling

Do not stop for minor ambiguity when the repository provides enough evidence to make a safe decision.

Resolve ambiguity using, in order:

1. current milestone acceptance criteria;
2. `docs/status.md`;
3. `docs/handoff.md`;
4. tests and existing regression coverage;
5. surrounding implementation patterns;
6. product contract and milestone documentation.

If several choices are valid, choose the smallest generic change that satisfies current acceptance criteria and preserves existing behavior.

Ask the user only when ambiguity materially changes product behavior, scope, or an irreversible decision.

### Keep the Repository Recoverable

Never leave PKC in an unexplained half-edited state.

After each meaningful internal checkpoint:

- know which files changed;
- know which verification passed;
- know what remains;
- keep temporary debugging changes identifiable;
- remove temporary instrumentation before completion.

If a session is interrupted, another session should be able to recover from:

```text
git status
git diff
docs/status.md
docs/handoff.md
```

For long tasks, update `docs/status.md` and/or `docs/handoff.md` at meaningful verified checkpoints, not after every tiny edit.

### Progress Heartbeat

During substantial work, surface concrete progress rather than becoming silent.

Useful progress updates report facts such as:

```text
root cause found
regression reproduced
implementation changed
focused tests green
broader tests running
new blocker discovered
blocker resolved
diff ready for review
```

Do not send repetitive status messages with no new information. Silence must not mean the task has stalled.

### Stuck-State Detection

Treat the task as potentially stuck when any of these occurs:

- the same command is executed repeatedly without new evidence;
- the same hypothesis is retried without modification;
- a process produces no meaningful progress;
- the agent waits for CI or an external service unnecessarily;
- the agent keeps inspecting files without choosing a next action;
- implementation changed but no focused verification follows;
- a failure is reported without diagnosis;
- the agent asks the user what to do when repository evidence already determines the answer.

When a stuck state is detected, reset the loop explicitly:

```text
state current evidence
→ identify the smallest unresolved question
→ choose one concrete action that answers it
→ execute that action
→ continue
```

## Critical Git / CI Rule

Do not commit or push after every small change.

GitHub Actions workflows are expensive. Multiple incremental pushes create unnecessary CI runs, consume runner capacity, and waste development time.

For one logical task, blocker, bug fix, or milestone checkpoint, prefer one final implementation commit and one push.

Bad workflow:

```text
edit
→ commit
→ push
→ wait for CI
→ edit
→ commit
→ push
→ wait for CI
→ edit
→ commit
→ push
```

Preferred workflow:

```text
inspect
→ edit locally
→ add regression tests
→ run focused tests locally
→ fix locally
→ run full relevant tests locally
→ review diff
→ commit once
→ push once
→ use CI as final verification
```

## Regression-First Work

When fixing a bug or review blocker:

1. reproduce the defect with a focused regression test;
2. confirm the regression represents the actual defect;
3. implement the generic fix;
4. run the regression locally;
5. run related tests locally;
6. run the full relevant test suite locally;
7. commit the regression and fix together unless there is a strong reason to preserve separate history;
8. push once after the complete fix is locally validated.

A separate red-test commit should only be pushed when explicitly required for audit or review evidence. Otherwise, proving the regression locally is sufficient.

## Commit Discipline

Commits must represent meaningful, coherent checkpoints.

Good examples:

```text
fix: enforce observable predicate authority
fix: qualify Angular service identity
test: cover same-line semantic invocation collision
docs: update V0.4.7 review handoff
```

Avoid commits such as:

```text
wip
try fix
fix test
fix again
oops
debug
```

Do not create commits merely to test whether CI passes.

## Push Discipline

Before pushing, verify:

- implementation is complete for the current logical task;
- focused regression tests pass locally;
- related tests pass locally;
- the repository builds locally;
- no temporary debugging code remains;
- the complete diff has been reviewed;
- documentation/status files are updated when required.

Then push once and let CI validate the completed checkpoint.

If CI fails because of a genuine environment-only or integration-only issue, investigate the failure before pushing another change. Do not repeatedly push speculative fixes.

## CI Philosophy

CI is the final verification layer, not the primary development loop.

Local execution should catch:

- compilation failures;
- unit test failures;
- regression failures;
- formatting or lint issues;
- obvious integration breakage.

GitHub Actions should primarily validate:

- clean-environment reproducibility;
- repository-wide gates;
- external-system tests;
- portable artifact validation;
- integration and acceptance workflows that cannot reasonably run locally.

Never use CI as a way to escape a local blocker.

After a push, CI must not block productive work that belongs to the same checkpoint. While CI runs, permissible work includes reviewing the committed diff, preparing review evidence, checking documentation consistency, and investigating remaining evidence required by the checkpoint. Do not start the next milestone until required gates pass.

## Main Branch Safety

When working directly on `main`:

- avoid partial implementations;
- avoid known-red commits;
- avoid temporary debugging commits;
- avoid pushing incomplete experiments;
- batch related changes into a locally verified checkpoint.

If an explicitly required regression-first audit needs a red commit, keep it exceptional and intentional, then follow it with the completed fix as efficiently as possible.

## Scope Discipline

Do not opportunistically refactor unrelated areas while fixing a blocker.

For each task:

```text
understand scope
→ reproduce problem
→ implement minimum generic fix
→ verify locally
→ review
→ commit
→ push
```

Stay within the current milestone and its acceptance criteria.

Keep these evidence classes distinct unless exact evidence proves composition:

- business conditions;
- value lineage/provenance;
- mutation/causality;
- API/wire identity;
- render authority;
- visibility authority;
- portable knowledge rendering.

Same or similar names are never proof of identity. Stronger composition failure must preserve independently proven lower-authority evidence.

## Status and Handoff

After completing a meaningful checkpoint:

- update `docs/status.md`;
- update `docs/handoff.md` when another session or reviewer needs the state;
- record exact HEAD, completed work, remaining blockers, and required next action;
- do not claim a milestone is complete until all required gates and reviews pass.

If full completion is impossible because of a genuine external blocker, complete every independent step that remains possible and leave an exact handoff containing:

- current HEAD;
- working tree state;
- completed work;
- exact failing command;
- exact failure;
- evidence collected;
- remaining blocker;
- next concrete action.

Never leave the handoff as merely `stuck` or `continue later`.

## Completion Discipline

Do not stop at `code written`.

A logical task is complete only after, where applicable:

```text
regression reproduced
+ implementation complete
+ focused verification passes
+ related verification passes
+ broader relevant tests/build pass
+ diff reviewed
+ temporary code removed
+ status/handoff updated
+ coherent commit created
+ push performed only when ready for CI
```

## Primary Principle

Optimize for:

```text
fewer pushes
+ stronger local validation
+ coherent commits
+ reliable CI
+ clear project history
+ continuous forward progress
```

not:

```text
many tiny commits
+ CI-driven debugging
+ repeated workflow runs
+ passive waiting
+ unexplained stuck states
```
