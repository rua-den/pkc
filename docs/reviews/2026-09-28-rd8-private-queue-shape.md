# RD8-B Repair (b) — Private Queue Shape Capture

Date: 2026-09-30
Status: **PARTIAL SANITIZED EVIDENCE / IMPLEMENTATION NOT AUTHORIZED**
Checkpoint: V0.4.7-E0 / RD8-B repair (b)

This file is the approved sanitized capture for the source-enabled inspection required before deferred command-queue linking is implemented.

Accepted evidence from `docs/reviews/2026-09-25-rd8-private-validation-result.md` is carried forward here. Unfilled fields remain unproven; examples, framework conventions and naming guesses are never evidence.

See also `docs/reviews/2026-09-30-rd8-private-queue-shape-reconciliation.md`.

## Privacy boundary

Record behavior and syntax/mechanism families only.

Do not record:

- private repository name or path;
- target file/class/method/type/member names;
- source bodies or copied source snippets;
- queue table/entity names;
- configuration values;
- raw PKC facts/relations;
- customer/business identities;
- secrets or credentials.

Invented labels such as `PRODUCER`, `QUEUE_RECORD`, `IDENTITY_MEMBER`, `DISPATCHER`, `RESOLVER`, and `HANDLER` are allowed.

## Capture state

```text
accepted prior queue evidence reconciled: YES
source-enabled inspection of missing hops: NO
full chain reviewed:                       NO
implementation authorized:                 NO
```

Implementation remains gated until the missing hops below are proven deterministically.

## 1. Producer identity write

Accepted sanitized evidence:

```text
producer callable shape:                 producer inserts a persisted queue row — PROVEN behavior-level
queue/message construction shape:        persisted queue row — PROVEN
handler identity representation family:  type-name string — PROVEN
exact type-name expression/encoding:      <UNFILLED>
identity stability basis:                <UNFILLED>
identity written directly or via helper: <UNFILLED>
```

The remaining inspection must establish the exact semantic expression that produces the already-proven type-name string. Do not assume `FullName`, `AssemblyQualifiedName`, `nameof`, or any other representation.

## 2. Persisted identity member

Accepted sanitized evidence establishes that the queue row carries the handler type-name string, but not the exact member or copy path.

```text
persisted member role:                 handler type-name identity — PROVEN role
exact persisted member:               <UNFILLED>
producer writes this exact member:     <UNFILLED>
dispatcher later reads same member:    <UNFILLED>
intermediate copy/transform present:   <UNFILLED>
transform preserves identity exactly:  <UNFILLED>
```

The capture must prove producer and dispatcher are connected by the same persisted identity, not by similar names or neighboring queue operations.

## 3. Dispatcher read + resolution

Accepted sanitized evidence:

```text
later dispatcher exists:              PROVEN
dispatcher later runs the handler:     PROVEN behavior-level
```

Still required:

```text
dispatcher trigger shape:             <UNFILLED>
identity read shape:                  <UNFILLED>
resolver mechanism family:            <UNFILLED>
resolver input equals persisted id:   <UNFILLED>
resolver output type/category:        <UNFILLED>
```

Do not infer recurring schedule semantics here. Repair (c) owns scheduler-trigger evidence.

## 4. Handler selection

Fill after narrow source inspection:

```text
handler entry-point shape:             <UNFILLED>
callable fact already representable:   <UNFILLED>
number of matching concrete handlers:  <UNFILLED>
unique selection proven:               <UNFILLED>
selection proof basis:                 <UNFILLED>
```

A concrete `deferred-dispatch` edge is authorized only when exactly one callable handler is proven through the full identity chain.

## 5. Ambiguity / unsupported cases

Regression coverage must fail closed for:

```text
mismatched persisted identity
missing consumer/dispatcher
unsupported resolver family
ambiguous handler identity
duplicate/non-unique callable fact
```

Expected behavior:

- no guessed handler edge for any incomplete or ambiguous chain;
- retain `unresolved-deferred-dispatch` only when the producer-side queued identity is itself grounded;
- no inference from simple names, suffixes, matching method names, folder proximity, unrelated queue operations or framework convention.

## 6. Sanitized structural chain

Current evidence state:

```text
PRODUCER
  -> type-name string [PROVEN representation family; exact encoding UNPROVEN]
  -> QUEUE_RECORD.IDENTITY_MEMBER [persisted role PROVEN; exact member UNPROVEN]
  -> DISPATCHER exists and later runs handler [PROVEN behavior-level]
  -> same-member read [UNPROVEN]
  -> resolver mechanism [UNPROVEN]
  -> exactly one HANDLER callable [UNPROVEN]
```

Source-enabled inspection should fill only the `UNPROVEN` hops.

## 7. Regression fixture contract

Fill only after the remaining structural chain is reviewed:

```text
positive fixture can reproduce authorized mechanism without private identifiers: <UNFILLED>
negative: mismatched identity:                                           REQUIRED
negative: missing consumer:                                              REQUIRED
negative: ambiguous handler:                                             REQUIRED
negative: unsupported resolver:                                          REQUIRED
negative: duplicate callable fact:                                       REQUIRED
```

The synthetic fixture must use invented names and only the sanitized mechanism family actually confirmed.

## 8. Implementation authorization decision

```text
NOT AUTHORIZED
```

Reason:

```text
Queue persistence, producer insertion, type-name-string handler identity, and later dispatcher execution are accepted evidence.
The shareable record still lacks exact identity encoding/member continuity, resolver mechanism and unique-callable proof.
```

Do not write production code that guesses those missing mechanisms.

## 9. Exact next inspection

Approved source-enabled/company environment only:

```text
1. confirm exact type-name expression/encoding family;
2. confirm exact queue-record member carrying it;
3. prove dispatcher reads that same member;
4. record resolver mechanism family;
5. prove callable entry-point selection and uniqueness;
6. mark AUTHORIZED only if the complete chain is deterministic.
```

No private source contents or raw facts belong in this file.
