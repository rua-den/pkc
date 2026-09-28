# RD8-B Repair (b) — Deferred Command-Queue Linking

Date: 2026-09-28
Status: approved direction; implementation plan recorded; implementation not started.

## Goal

Recover a grounded path from a deferred command-queue producer to its handler when the repository proves the same handler identity through queue persistence and dispatch. Keep this evidence distinct from synchronous calls and recurring-job trigger evidence.

## Evidence and scope

The sanitized RD8-B baseline records that a producer persists a handler type-name string in a queue row and a later dispatcher invokes the handler. The missing path affected the scheduled-update and work-log probes. Repair (a), conditional state-effect calibration, is closed by the 2026-09-28 private re-probe; the overall RD8-B result remains PARTIAL/NOT PASS.

The source syntax family and the complete producer → persisted identity → dispatcher resolution → handler chain must be confirmed in the approved source-enabled environment before implementation. This repository stores only a sanitized structural description, never target paths, names, code bodies, or raw facts.

## Required behavior

1. Prove the producer writes a stable handler identity into a queued command/message.
2. Prove the dispatcher reads and resolves that same identity and reaches one concrete handler entry point in the scanned solution.
3. Emit a deferred-dispatch relation from the producer to the handler only when every identity hop is deterministic and the final handler is unique.
4. Keep the deferred relation visibly distinct from `invokes`, direct DI `dispatches`, and sole-implementation dispatch. Its wording describes a queued route, not successful or timed runtime execution.
5. If a producer-side identity is observed but the consumer/resolution/handler chain is missing or ambiguous, preserve an unresolved-deferred-dispatch unknown. Do not guess from simple names, suffixes, folder proximity, matching method names, or unrelated queue operations.
6. Do not infer recurring schedules, retry behavior, delivery guarantees, execution timing, or business intent. Repair (c) remains separate.

## Design boundary

Extend the existing C# project-semantic relation path and product-feature flow rendering only as far as needed for the confirmed queue shape. Reuse Roslyn symbol identity and the loaded solution model. Retain both ends of the evidence chain: the producer callable is the relation source, and the relation source location points to the dispatcher resolution/call site. If the confirmed shape cannot be linked with the existing fact/relation model without losing required provenance, revise this design before implementation rather than weakening the proof boundary.

Proposed relation kinds:

```text
deferred-dispatch
unresolved-deferred-dispatch
```

The first links the producer fact ID to one concrete handler fact ID. The second records a known producer-side queue identity whose handler cannot be uniquely proven. Neither relation asserts runtime execution.

## Acceptance criteria

- A synthetic positive fixture using the sanitized queue/dispatcher shape links one producer to exactly one handler through an exact identity chain.
- The rendered feature flow labels the edge as deferred and does not render it as a synchronous method invocation.
- A missing consumer, mismatched queue identity, unsupported resolver, ambiguous handler identity, or duplicate callable fact produces no guessed producer-to-handler edge.
- An unresolved case is described as unresolved rather than silently omitted when producer-side evidence is available.
- Existing direct invocation, DI-dispatch, sole-implementation and unresolved-dispatch behavior remains unchanged.
- Repairs (a) and (c), unrelated semantic gaps, and target-specific names remain out of scope.

## Privacy boundary

All implementation and tests use invented fixture names. Source inspection is read-only and stays in the approved source-enabled environment. PKC repository documentation receives only the sanitized behavior pattern and score; no target code, paths, identities, raw facts, or secrets.
