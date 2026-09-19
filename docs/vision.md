# Vision

PKC is a **Product/System Knowledge Compiler**.

Its goal is to turn implementation and delivery evidence into a portable knowledge pack that a Product Owner can give to any AI assistant. The expensive work of understanding the system should happen during compilation, not every time somebody asks a product question.

## Product goal

Long-term inputs:

- backend/source code;
- frontend/UI implementation and eventually runtime UI evidence;
- eventually Azure DevOps Epics, Features, PBIs, Sprints and history.

PKC should generate knowledge that explains:

- what the system is;
- what features exist;
- how users perform workflows;
- business rules, validations, permissions and state transitions;
- side effects and integrations;
- where important values came from and how they change;
- how backend values travel through DTO/API/frontend layers;
- when/why a feature was delivered once delivery-history evidence is integrated;
- known gaps, inconsistencies and possible improvements;
- evidence supporting each important statement.

## Success and current boundaries

The intended daily experience is:

```text
pkc build <repository>
→ give generated knowledge to an AI
→ ask product/system questions without rescanning source
```

Success means the answer is useful, accurate inside the supported scope, portable and traceable. Fact counts and analyzer features are intermediate means, not the product outcome.

Keep these knowledge classes distinct:

```text
business conditions
value lineage / provenance
mutation / causality
```

Missing authority must not delete independently known evidence. Unsupported behavior must remain explicitly unproven; a supported positive checkpoint cannot pass merely by answering everything with unknown.

## Current accepted scope

Accepted scope through **V0.4.7-C** includes:

- C#/.NET project-semantic evidence with conservative fallback behavior;
- supported static frontend evidence and backend/frontend workflow linkage;
- conservative business-condition authority and retained observed-only evidence;
- portable feature/workflow Markdown and one-file/ZIP handoff;
- bounded stored scalar copy/snapshot lineage;
- bounded reference/dynamic read-time lineage;
- bounded multi-input scalar derivation with snapshot semantics;
- later supported override/mutation causality retained separately from original origin;
- last proven source before a supported direct return boundary;
- exact target-project-semantic backend entity/domain property → explicit DTO/response property → API-response lineage;
- renamed DTO property support only when semantic assignment proves the edge;
- exact project/assembly/type/member identity and source/projection/response locations for the supported C chain;
- fail-closed authority across aliases, opaque/custom effects, unsupported control flow, same-name collisions and same full symbol names across assemblies;
- PokeTrade known-answer, Loren pinned + moving canary and pinned Jellyfin generalization/parity gates.

Accepted final C code:

```text
fbb64b9917da1f63362558355201ff7998384ba0
feat: prove backend API projection lineage
```

V0.4.7-D is the current checkpoint. It extends the proven API value path through exact frontend HTTP/result → component/view-model state → rendered/displayed value, then composes backend/frontend visibility only for the same proven item/dataflow path. E is final knowledge-only product acceptance.

See `docs/status.md`, `docs/milestones.md` and `docs/v0.4.7-acceptance-plan.md` for the authoritative checkpoint boundaries.

## Important non-goals at the current checkpoint

PKC is not trying to become a general symbolic execution engine, alias solver or arbitrary effect/dataflow solver. Each accepted capability has an explicit supported proof boundary and fails closed outside it.

Accepted C support does not imply arbitrary helper projection analysis, custom accessor semantics, constructor mapping, response-wrapper unwrapping or name-based DTO matching.

D must not silently promote syntax-only frontend evidence into semantic identity. If exact frontend receiver/result/binding identity cannot be proven inside the supported boundary, the stronger cross-layer edge must remain unknown while accepted backend evidence remains available.

Azure DevOps history, incremental compilation, runtime UI confirmation and product insight/drift analysis are future capabilities. Do not describe them as available today. Delivery-history answers require the future Azure DevOps input.

## Example

A PO should eventually be able to ask:

> How do I change the status of a WorkPlay? What should I be careful about?

and receive a source-traceable explanation of user steps, valid transitions, validations, permissions, side effects, value origins/changes and — once Azure DevOps is integrated — delivery history, without scanning the repository again.

A cross-layer value question should likewise become answerable from compiled knowledge:

> Which backend property supplies this API field, how was it computed or overridden, and where is it used in the UI?

A and B cover backend origin/timing/computation/change. C now closes backend entity/domain → DTO/API delivery. D is current for API → frontend binding and joint visibility.

## Core principles

1. **Portable output** — Markdown/YAML is the product interface. No specific AI vendor is required.
2. **Evidence before prose** — source is first converted into deterministic facts; an LLM must not directly rewrite an entire repository into documentation.
3. **Traceability** — important knowledge points back to source evidence.
4. **Fail closed** — unsupported inference remains unknown rather than becoming a false PO claim.
5. **Retain lower-authority evidence** — losing stronger composition authority must not erase deterministic origin/mutation evidence.
6. **Incremental compilation is a future efficiency goal** — planned for V0.6 after product-readiness gates.
7. **Current behavior is not automatically business intent** — implementation evidence and approved product intent remain distinguishable.
8. **Cheap daily use** — PO questions use compiled knowledge rather than repeatedly rescanning source.

## Long-term compiler shape

```text
Backend / Source ──┐
Frontend / UI ─────┼──> Evidence model ──> Feature synthesis ──> Knowledge model ──> Markdown
Azure DevOps ──────┘
```

V0.1 began with C# evidence. Delivery continues one usable checkpoint at a time. Every checkpoint needs a PO question, a generated answer artifact, explicit correctness/fail-closed gates and a stop condition; it must not become an open-ended analyzer project.
