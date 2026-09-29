# RD8-B Repair (b) — Deferred Queue Evidence Reconciliation

Date: 2026-09-30
Checkpoint: V0.4.7-E0 / RD8-B repair (b)
Status: **PARTIAL SANITIZED EVIDENCE / IMPLEMENTATION NOT AUTHORIZED**

This note reconciles the repair-(b) source-shape gate with private evidence that was already accepted on 2026-09-25. It does not add new private-source knowledge and does not authorize implementation by itself.

Authoritative accepted source-known evidence:

`docs/reviews/2026-09-25-rd8-private-validation-result.md`

## What is already proven

The accepted private validation states, at sanitized behavior level:

```text
producer
  -> inserts a queue row
  -> the row names its handler by a type-name string
  -> a dispatcher later runs the handler
```

Therefore repair (b) does **not** need to rediscover whether the deferred identity is a string or whether a persisted queue boundary exists. Those points are already accepted evidence.

Safe carry-forward facts:

```text
queue persistence exists:             PROVEN
producer creates/inserts queue row:   PROVEN
handler identity representation:      type-name string — PROVEN
later dispatcher exists:              PROVEN
producer -> handler direct call:      NOT CLAIMED
runtime execution/delivery success:   NOT CLAIMED
schedule/timing/retry semantics:      NOT CLAIMED
```

## What remains unproven in the shareable record

The accepted 2026-09-25 note intentionally did not retain enough implementation detail to reproduce the resolver generically. The following still require narrow read-only confirmation in the approved source-enabled environment:

```text
exact type-name string expression/encoding family
exact persisted member carrying that string
proof dispatcher reads that same persisted member
resolver mechanism that maps the string to a runtime type/service/handler
callable entry-point selection from the resolved handler
whether final handler selection is unique and deterministic
```

Examples such as `FullName`, `AssemblyQualifiedName`, `Type.GetType`, assembly scanning, DI resolution, a dictionary/factory or reflection invocation are **not assumptions**. Record only the mechanism actually observed.

## Narrow inspection contract

Do not re-audit unrelated business logic. Inspect only enough source to answer these questions:

1. How is the already-proven type-name string produced?
2. Which queue-record member receives it?
3. Does the dispatcher later read that exact member without an identity-changing transform?
4. How is that string resolved to a type/service/handler?
5. How is the callable handler method selected?
6. Can exactly one callable handler be proven from this chain?

Use invented labels in the shareable result:

```text
PRODUCER
QUEUE_RECORD.TYPE_IDENTITY
DISPATCHER
RESOLVER
HANDLER
```

Do not commit private names, paths, source snippets, raw facts or configuration values.

## Implementation authorization rule

Implementation becomes `AUTHORIZED` only if the source-enabled inspection can complete this deterministic chain:

```text
PRODUCER
  -> proven type-name string expression
  -> proven QUEUE_RECORD member
  -> DISPATCHER reads same member
  -> supported deterministic RESOLVER
  -> exactly one callable HANDLER
```

If the producer-side type-name string remains proven but one later hop is missing or ambiguous, the intended product behavior is fail-closed:

```text
unresolved-deferred-dispatch
```

Do not emit a concrete `deferred-dispatch` edge.

## Current decision

```text
source-enabled inspection performed for missing hops: NO
full chain reviewed:                              NO
implementation authorized:                        NO
```

Reason: the accepted evidence proves queue persistence + type-name-string identity + later dispatch, but the shareable repository still lacks deterministic resolver and unique-callable proof.

## Exact next action

In the approved source-enabled/company environment:

```text
confirm only the missing identity-member/resolver/callable hops
-> update docs/reviews/2026-09-28-rd8-private-queue-shape.md with sanitized mechanism families
-> mark AUTHORIZED only if the full chain is deterministic
-> execute docs/superpowers/plans/2026-09-28-rd8-deferred-command-queue-linking.md regression-first
```
