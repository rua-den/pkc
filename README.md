# PKC — Product Knowledge Compiler

PKC turns implementation evidence into portable product knowledge that a Product Owner can attach to ChatGPT, Claude, Gemini, Copilot or another capable AI.

The core success condition is not “the scanner found many facts”. It is:

> Can an AI receive only the generated `knowledge/` pack and explain the product/system accurately, at the right abstraction level, without re-reading the source repository?

PKC follows an evidence-first compiler model:

```text
source
  ↓
deterministic analyzers / adapters
  ↓
.pkc/facts.json
  ↓
workflow candidates
  ↓
canonical knowledge
  ↓
portable Markdown
```

Important claims are grounded in source evidence. PKC does not silently invent UI behavior, delivery history or requirements that were not analyzed.

## Current roadmap state

```text
V0.4.6 business logic reconstruction       PASS / COMPLETE
V0.4.7 cross-layer PO-question readiness   IN PROGRESS
V0.5 Azure DevOps input evidence           LOCKED
```

Accepted V0.4.6 production:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Final independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-10.md
PASS / COMPLETE
```

V0.4.6 is closed. V0.4.7 is current. Azure DevOps ingestion remains locked until the V0.4.7 / V0.4.x Product Owner question-readiness exit gate passes.

The V0.4.7 acceptance contract is defined in:

```text
docs/v0.4.7-acceptance-plan.md
```

Checkpoint A's direct scalar copy/snapshot slice is implemented and exact-SHA verified through `bc938823b46802a4d2c32300a1b6de692f5866ad`. It proves `ProductGroup.Price → Product.Price → Service.Price` for the bounded supported shape, preserves source traceability, and fails closed on stale chain composition after receiver reassignment or opaque mutation. The current next deliverable is A's separate **reference/dynamic-read positive**; A is not complete until that behavior and its portable PO-facing answer pass.

Every V0.4.7 checkpoint must deliver knowledge that answers its PO questions. A covers origin/copy timing, B computation/later changes, C DTO/API output, D frontend binding/visibility, and E final knowledge-only acceptance and portable parity. Rendering starts in A. Fact counts or all-unknown answers do not establish success. See [the roadmap](docs/milestones.md) for completion boundaries and [current status](docs/status.md) for accepted versus pending work.

## Product-knowledge contract

PKC keeps three knowledge classes distinct:

```text
business conditions
value lineage / provenance
mutation / causality
```

They can be connected in a PO-facing explanation, but one class must not be silently promoted into another.

Conservative authority downgrade is preferred over a false product claim. Downgrading authority must not erase deterministic lower-authority lineage, mutation or causal evidence.

Detailed contract: `docs/product-knowledge-contract.md`.

## Knowledge hierarchy

PKC preserves detailed evidence while presenting product knowledge in layers:

```text
knowledge/index.md
  → orient the AI around observed capabilities and boundaries

knowledge/features/**/*.md
  → capability-level rules, permissions, outcomes and important failures

knowledge/workflows/**/*.md
  → operation-level behavior, validations, state changes, side effects, flow and evidence

.pkc/facts.json
  → detailed implementation evidence and analyzer provenance
```

A transitive helper guard/loop is not automatically a product rule. Detail should be promoted only when it materially affects observable behavior, constraints, outcomes or safety.

## What works today

Current accepted/development scope includes:

- C#/.NET evidence with Roslyn, preferring the target project's real `MSBuildWorkspace` compilation;
- explicit C# fallback when target-project semantic context is unavailable;
- MVC-style and Minimal API backend evidence, including permissions, guards, throws, direct/failure responses, mutations and call relations;
- conservative business-predicate extraction and Product Owner authority filtering;
- provider-aware Queryable fail-closed behavior: exact Queryable predicates are observed-only unless provider semantics are independently proven;
- retention of downgraded predicate evidence through `observes-predicate` rather than deletion;
- bounded project-semantic scalar value-transfer evidence for direct auto-property copies;
- stored snapshot lineage rendering with exact receiver/member/value-version composition checks;
- conservative stale-composition invalidation for receiver reassignment, opaque invocation barriers and tracked unary writes;
- Angular TypeScript structure/routes/HTTP-call shapes through the project-local TypeScript syntactic AST when available;
- Angular template actions through an explicit conservative template-regex fallback;
- React/TypeScript through an explicit conservative regex fallback;
- framework-agnostic frontend adapter boundary (`IFrontendAdapter`);
- UI action → API call → backend endpoint linkage;
- UI field, validation, visibility/list behavior and frontend/backend validation consistency for currently supported shapes;
- workflow Markdown, product-feature Markdown and `knowledge/index.md`;
- portable `PKC_KNOWLEDGE.md` and `PKC_KNOWLEDGE.zip` handoff artifacts;
- PokeTrade known-answer runnable regression;
- Loren pinned real-project acceptance plus Loren-main moving canary;
- Jellyfin pinned independent generalization acceptance with portable parity/no-leak verification.

Runtime browser confirmation is not implemented yet.

V0.4.7 generalized reference/dynamic dependencies, derivations, later override/mutation causality, DTO/API/frontend value lineage and joint cross-layer visibility remain pending. The regression contract is defined in `docs/v0.4.7-acceptance-plan.md`.

## V0.4.7 acceptance focus

The current milestone must make portable knowledge answer practical questions such as:

```text
Where did this value originally come from?
If the upstream value changes later, does the existing downstream value change automatically?
What code path can change this value after creation?
Was this value directly copied or computed?
What was the last observed source before the value was persisted or returned?
What backend conditions and frontend conditions jointly determine whether an item is visible?
How did the value move through backend → DTO/projection → API → frontend composition?
If authority is incomplete, what lineage or causal evidence is still deterministically known?
```

Canonical acceptance lineage starts with:

```text
ProductGroup.Price
→ Product.Price
→ Service.Price
```

Each proven edge must distinguish, as applicable:

```text
copy
snapshot
derivation
reference/dynamic
override
mutation
```

A blocking negative regression also uses unrelated same-name members:

```text
Product.Price
Service.Price
Dto.Price
Component.price
```

PKC must never connect lineage merely because names match.

## Analyzer fidelity

Analyzer fidelity is part of the evidence and is not hidden behind a generic "static analysis" label.

Current modes include:

```text
project-semantic                  high/medium
  C# target-project MSBuildWorkspace context. Declaration node matches are high confidence;
  project-loaded-but-node-match-failed evidence is reduced to medium.

typescript-ast-syntactic          high/medium
  Angular uses the target repo's local TypeScript parser via ts.createSourceFile.
  Structural ui-screen/ui-route evidence is high confidence.
  ui-api-call evidence is medium because receiver type is not checked by a TypeChecker.

loose-roslyn-fallback             medium
  C# source analyzed without the full target-project reference graph.

angular-template-regex-fallback   medium
  Angular template action/visibility extraction.

regex-fallback                    low
  conservative text-pattern fallback, currently including React and Angular when the TS AST path is unavailable.
```

Angular `ui-api-call` facts additionally carry:

```text
typescriptSemanticContext: syntax-only-no-type-checker
httpReceiverResolution: syntactic-unverified
```

### Angular AST runtime precondition

The higher-fidelity Angular TypeScript path currently requires:

1. `node` available on `PATH`;
2. a project-local `node_modules/typescript/lib/typescript.js`, normally produced by the repository dependency-install step.

If unavailable, PKC records an explicit fallback rather than hiding the downgrade.

## Version semantics

PKC has independent version domains. They must not be mechanically synchronized.

### Roadmap milestone version

Examples:

```text
V0.4.6
V0.4.7
V0.5
```

This tracks roadmap and acceptance progress.

### Tool/package version

The last accepted packaged checkpoint remains:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

This is the distributable .NET tool version. Moving the roadmap to V0.4.7 does not automatically bump it.

### Evidence/schema version

Serialized documents have their own contract identifiers. Existing examples include:

```text
0.4.4-csharp-raw
0.4.4
0.4.6
0.4.3-frontend
```

These are schema/contract versions, not the current roadmap milestone.

Do not mechanically change an existing schema string because V0.4.7 is current. A schema version changes only when the serialized contract or semantics for that artifact change, with explicit compatibility and regression coverage.

## Quickstart from source

Until a new public package/release is explicitly accepted, PKC can be packed and installed locally at the accepted package version:

```bash
dotnet pack src/Pkc.Cli/Pkc.Cli.csproj -c Release -o ./artifacts/tool
dotnet tool install --tool-path ./.pkc-tool --add-source ./artifacts/tool RuaDen.Pkc.Tool --version 0.4.3-preview.2
./.pkc-tool/pkc build /path/to/your/repository
```

For development from source:

```bash
dotnet run --project src/Pkc.Cli/Pkc.Cli.csproj -- build <repository-path>
```

Use `scan` when only machine evidence/candidates are needed:

```bash
pkc scan <repository-path>
```

## Output and AI handoff

`pkc build <repository-path>` keeps the structured knowledge pack and also produces portable handoff artifacts:

```text
.pkc/
  facts.json
  feature-candidates.json
  product-features.json

knowledge/
  AI_INSTRUCTIONS.md
  index.md
  features/
    <area>/
      <feature>.md
  workflows/
    <area>/
      <workflow>.md

PKC_KNOWLEDGE.md
PKC_KNOWLEDGE.zip
```

Recommended usage:

```text
single-file AI upload
→ upload PKC_KNOWLEDGE.md

AI/workspace that supports multiple files
→ provide the knowledge/ pack
→ AI starts from AI_INSTRUCTIONS.md then index.md

archive-capable destination or storage/sharing
→ use PKC_KNOWLEDGE.zip
```

The ZIP is a transport convenience, not a required AI interface. Archive support varies by destination. `PKC_KNOWLEDGE.md` exists so a PO can use a one-file handoff without understanding PKC's internal folder layout.

The ZIP contains only the portable `knowledge/` files by default; it does not include source code or raw `.pkc/facts.json`.

Generated knowledge is currently `code-observed`. Requirement intent and delivery history remain unknown until those evidence sources are explicitly added.

Detailed handoff contract: `docs/ai-handoff.md`.

## Validation strategy

PKC deliberately uses multiple benchmark roles:

```text
PokeTrade
→ known-answer runnable regression

Loren pinned commit
→ accepted V0.4.4 real-project knowledge-readiness benchmark

Loren main
→ moving non-blocking canary

Jellyfin pinned commit
→ accepted V0.4.5 independent anti-overfit/generalization benchmark

V0.4.7 focused fixtures
→ current cross-entity/cross-layer PO-question-readiness regression surface
```

V0.4.4 and V0.4.5 are historical accepted gates, not current unfinished work. V0.4.6 is also accepted and closed.

A CI/grep suite is necessary but does not by itself prove V0.4.7 PO-question readiness. V0.4.7 must add focused semantic regressions, portable-answer validation and cross-benchmark verification before the V0.4.x exit gate can pass.

V0.5 Azure DevOps remains locked until V0.4.7 and the V0.4.x PO-question-readiness exit gate pass.

Detailed historical/current exit plan: `docs/real-project-trial.md`.

## V0.4.6 non-blocking warnings carried forward

```text
W10.1
Observed-only predicate evidence is separated from Rules but rendered Evidence does not explicitly print `observed-only`.

W10.2
Queryable names remain in old safe-operation sets but are unreachable behind the Queryable fail-closed guard.
```

These are V0.4.7 maintenance/clarity considerations. They do not reopen V0.4.6.

## Planned, not implemented yet

- V0.4.7 reference/dynamic-read dependency proof and temporal explanation;
- generalized derived/computed lineage and later override/mutation causality;
- generalized backend → DTO/projection → API → frontend value composition;
- generalized joint backend/frontend visibility explanation;
- TypeScript `Program` / `TypeChecker` semantic analysis for Angular receiver/type resolution unless required by a proven V0.4.7 gap;
- React AST-backed analysis unless required by a proven gap;
- full Angular template AST/compiler analysis unless required by a proven gap;
- ASP.NET MVC / Razor Pages, Blazor and Vue frontend adapters;
- Azure DevOps Epic / Feature / PBI / Sprint evidence;
- incremental compilation and PR knowledge diffs;
- runtime UI confirmation;
- product gap/drift analysis;
- optional LLM-assisted synthesis where deterministic grouping is insufficient;
- optional live delivery adapters such as MCP/connector/workspace synchronization after the portable knowledge contract is proven.

These are roadmap items, not automatic V0.4.7 work. A new analyzer capability enters the milestone only when a concrete acceptance regression proves it is required.

## Project status

`docs/status.md` is the current source of truth. `docs/handoff.md` is the continuation state. `docs/v0.4.7-acceptance-plan.md` defines the current acceptance/regression contract. `docs/real-project-trial.md` preserves the V0.4.x real-project exit history and current V0.5 unlock gate. `docs/milestones.md` defines the roadmap gates.

See also:

- `docs/vision.md`
- `docs/architecture.md`
- `docs/product-knowledge-contract.md`
- `docs/status.md`
- `docs/handoff.md`
- `docs/milestones.md`
- `docs/v0.4.7-acceptance-plan.md`
- `docs/real-project-trial.md`
- `docs/ai-handoff.md`
- `docs/golden-output.md`
