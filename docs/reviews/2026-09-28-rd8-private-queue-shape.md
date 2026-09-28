# RD8-B Repair (b) — Private Queue Shape Capture

Date: 2026-09-28
Status: **TEMPLATE / NOT EVIDENCE**
Checkpoint: V0.4.7-E0 / RD8-B repair (b)

This file is the approved sanitized capture shape for the source-enabled inspection required before deferred command-queue linking is implemented.

Do **not** treat unfilled fields, examples, framework conventions, naming guesses or generic queue patterns as evidence. This document becomes evidence only after the approved source-enabled/company inspection fills the structural fields below and explicitly marks the capture reviewed.

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

Invented labels such as `PRODUCER`, `QUEUE_RECORD`, `IDENTITY_MEMBER`, `DISPATCHER`, `RESOLVER`, and `HANDLER` are allowed when needed to explain the shape.

## Capture state

```text
source-enabled inspection performed: NO
capture reviewed:                  NO
implementation authorized:         NO
```

Implementation remains gated while any value above is `NO`.

## 1. Producer identity write

Fill after inspection:

```text
producer callable shape:                 <UNFILLED>
queue/message construction shape:        <UNFILLED>
handler identity expression family:      <UNFILLED>
identity stability basis:                <UNFILLED>
identity written directly or via helper: <UNFILLED>
```

Questions to answer:

- What semantic expression creates the handler identity?
- Is that identity compiler/symbol-grounded, a stable runtime type identity, or something else deterministic?
- Can PKC prove the expression belongs to the producer callable rather than a neighboring helper/site?

Fail closed if the producer-side identity itself cannot be grounded.

## 2. Persisted identity member

Fill after inspection:

```text
persisted member role:                 <UNFILLED>
producer writes this exact member:     <UNFILLED>
dispatcher later reads same member:    <UNFILLED>
intermediate copy/transform present:   <UNFILLED>
transform preserves identity exactly:  <UNFILLED>
```

The capture must prove the producer and dispatcher are connected by the same persisted identity, not by similar member names or nearby queue operations.

## 3. Dispatcher read + resolution

Fill after inspection:

```text
dispatcher trigger shape:             <UNFILLED>
identity read shape:                  <UNFILLED>
resolver mechanism family:            <UNFILLED>
resolver input equals persisted id:   <UNFILLED>
resolver output type/category:        <UNFILLED>
```

Do not infer recurring schedule semantics here. Repair (c) owns scheduler-trigger evidence.

## 4. Handler selection

Fill after inspection:

```text
handler entry-point shape:             <UNFILLED>
callable fact already representable:   <UNFILLED>
number of matching concrete handlers:  <UNFILLED>
unique selection proven:               <UNFILLED>
selection proof basis:                 <UNFILLED>
```

A concrete `deferred-dispatch` edge is authorized only when exactly one callable handler is proven through the full identity chain.

## 5. Ambiguity / unsupported cases observed

Fill after inspection:

```text
missing-consumer possibility:          <UNFILLED>
identity mismatch possibility:         <UNFILLED>
unsupported resolver variants:         <UNFILLED>
duplicate/ambiguous handlers possible: <UNFILLED>
```

Expected fail-closed behavior:

- no guessed handler edge for any incomplete/ambiguous chain;
- retain `unresolved-deferred-dispatch` only when producer-side queued identity is itself proven;
- no inference from simple names, suffixes, matching method names, folder proximity, unrelated queue operations or framework convention.

## 6. Sanitized structural chain

After inspection, write one behavior-only chain using invented labels:

```text
PRODUCER
  -> <identity expression family>
  -> QUEUE_RECORD.IDENTITY_MEMBER
  -> DISPATCHER reads the same identity
  -> <resolver mechanism family>
  -> exactly one HANDLER callable
```

If any hop is not proven, replace that hop with `UNPROVEN` and keep implementation unauthorized until the design is revised or evidence is sufficient.

## 7. Regression fixture contract

Fill only after sections 1-6 are reviewed:

```text
positive fixture can reproduce shape without private identifiers: <UNFILLED>
negative: mismatched identity:                                  <UNFILLED>
negative: missing consumer:                                     <UNFILLED>
negative: ambiguous handler:                                    <UNFILLED>
negative: unsupported resolver:                                 <UNFILLED>
```

The synthetic fixture must use invented names and only the sanitized mechanism family confirmed above.

## 8. Implementation authorization decision

One of:

```text
AUTHORIZED
  Full producer -> persisted identity -> dispatcher -> unique handler chain is proven and reproducible synthetically.

NOT AUTHORIZED
  At least one required identity/resolution hop is missing, ambiguous or cannot be represented without weakening provenance.
```

Current decision:

```text
NOT AUTHORIZED — template is unfilled; no source-enabled queue-shape inspection is recorded here yet.
```

## 9. Review record

Fill after source-enabled inspection:

```text
inspection date:            <UNFILLED>
reviewed sanitized capture: <UNFILLED>
implementation decision:    <UNFILLED>
remaining unproven hop:     <UNFILLED or NONE>
```

No private source contents or raw facts belong in this file.
