# PKC — Product Knowledge Compiler

PKC turns implementation evidence into portable product knowledge that a Product Owner can attach to ChatGPT, Claude, Gemini, Copilot or another capable AI.

The core success condition is not “the scanner found many facts”. It is:

> Can an AI receive only the generated knowledge pack and explain the product/system accurately, at the right abstraction level, without re-reading the source repository?

PKC follows an evidence-first compiler model:

```text
source
  ↓
deterministic analyzers / adapters
  ↓
.pkc/facts.json
  ↓
workflow / feature candidates
  ↓
canonical knowledge
  ↓
portable Markdown
```

Important claims are grounded in source evidence. PKC does not silently invent UI behavior, delivery history, value lineage or requirements that were not proven.

## Current roadmap state

```text
V0.4.6 business logic reconstruction       PASS / COMPLETE
V0.4.7-A origin and copy timing            PASS / COMPLETE
V0.4.7-B computation and later change      PASS / COMPLETE
V0.4.7-C backend → DTO/API lineage         PASS / COMPLETE
V0.4.7-D / R7.9 API → rendered value       PASS / COMPLETE
V0.4.7-D / R7.10 joint visibility          CURRENT / UNLOCKED
V0.4.7-E product acceptance                LOCKED behind D
V0.5 Azure DevOps input evidence           LOCKED
```

Accepted final V0.4.7-C production code:

```text
fbb64b9917da1f63362558355201ff7998384ba0
feat: prove backend API projection lineage
```

Accepted R7.9 implementation/test checkpoint:

```text
fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a
test: target frontend casing collision
```

V0.4.7-D remains open only for R7.10 joint backend/frontend visibility and the remaining D review/gates. Azure DevOps ingestion remains locked until V0.4.7 and the V0.4.x PO-question-readiness exit gate pass.

Acceptance contract: `docs/v0.4.7-acceptance-plan.md`.
Current source of truth: `docs/status.md`.
Continuation state: `docs/handoff.md`.

## Product-knowledge contract

PKC keeps three knowledge classes distinct:

```text
business conditions
value lineage / provenance
mutation / causality
```

They may be connected in a PO-facing explanation, but one class must not be silently promoted into another.

Conservative authority downgrade is preferred over a false product claim. Downgrading authority must not erase deterministic lower-authority lineage, mutation or causal evidence.

Detailed contract: `docs/product-knowledge-contract.md`.

## What works today

Current accepted scope includes:

- C#/.NET evidence with Roslyn, preferring the target project's real `MSBuildWorkspace` compilation;
- explicit C# fallback when target-project semantic context is unavailable;
- MVC-style and Minimal API backend evidence including permissions, guards, throws, direct/failure responses, mutations and calls;
- conservative business-predicate extraction and Product Owner authority filtering;
- provider-aware Queryable fail-closed behavior;
- retention of downgraded predicate evidence rather than deletion;
- Angular TypeScript structure/routes/HTTP-call shapes through project-local bounded analysis;
- Angular template actions/visibility through explicit conservative fallback where stronger evidence is unavailable;
- React/TypeScript through conservative fallback;
- framework-agnostic frontend adapter boundary (`IFrontendAdapter`);
- UI action → API call → backend endpoint linkage;
- explicit-wire bounded API-response-field → typed frontend result → component state → rendered-value lineage;
- workflow Markdown, product-feature Markdown and `knowledge/index.md`;
- portable `PKC_KNOWLEDGE.md` and `PKC_KNOWLEDGE.zip` handoff artifacts;
- PokeTrade known-answer runnable regression;
- Loren pinned real-project acceptance plus Loren-main moving canary;
- Jellyfin pinned independent generalization with portable parity/no-leak verification.

### Accepted V0.4.7 value-lineage scope through R7.9

PKC supports bounded evidence for:

```text
stored direct copy
stored snapshot timing
reference/dynamic read-time dependency
multi-input scalar derivation
later supported override / mutation causality
last proven source before a supported direct return
backend entity/domain property → explicit DTO/response property → API response
explicit API wire field → typed frontend result → component/view-model state → rendered value
```

Canonical examples:

```text
ProductGroup.Price
→ Product.Price
→ Service.Price

ProductGroup.Price
→ Service.CurrentGroupPrice  // supported dynamic/read-time dependency

Service.Price + Service.Discount
→ Service.NetPrice           // stored derivation

ProductEntity.Price
→ PriceResponse.DisplayPrice // explicit semantic DTO/API projection
→ wire name displayPrice
→ PriceResult.displayPrice
→ PriceComponent.displayPrice
→ rendered displayPrice
```

A stored derivation is a point-in-time snapshot. Later changes to its inputs do not rewrite the stored value without another proven mutation.

C's DTO/API edge is based on exact target-project semantic assignment and project/assembly/type/member identity. R7.9 then requires an explicit semantic JSON wire contract plus exact bounded frontend service/result/assignment/render identity inside its supported proof grammar. Same/equivalent names alone never create lineage.

R7.9 does **not** claim general TypeScript TypeChecker semantics. Its accepted evidence modes explicitly describe the bounded frontend proof used; unsupported shapes fail closed rather than being promoted by convention.

### Fail-closed behavior

Permanent accepted regressions through R7.9 include:

- nested/deconstruction property writes and deconstruction aliases;
- compound/unary writes;
- opaque invocation;
- custom getter/setter/constructor effects;
- user-defined operator/conversion effects;
- reference aliases/reassignment/reference parameters;
- unsupported branch/loop/try/conditional and `goto`/label/throw control flow;
- unresolved target-project semantic context;
- unrelated same-name DTO/entity properties;
- same-name namespace collisions;
- same full type/member name across different assemblies;
- custom source/target DTO accessors and unsupported projection effects;
- frontend `DisplayPrice` / `displayPrice` collisions without explicit wire proof;
- unrelated frontend services/results that expose the same member;
- unresolved/ambiguous frontend service or result receivers;
- untyped/fallback-only HTTP evidence;
- non-authoritative render evidence.

Unsupported shapes fail closed while independently proven lower-authority evidence remains available.

This is deliberately **not** a general symbolic execution, alias, effect, arbitrary helper-projection, persistence or TypeScript semantic solver.

## Final R7.9 verification

Exact-SHA gates on `fc4bfa7042f59620b2c7ba1c4f6700cf3b02172a`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35588410936 — PASS
pinned Loren                                35588410941 — PASS
Loren-main canary                           35588410930 — PASS
pinned Jellyfin                             35588410939 — PASS
```

Core:

```text
Release build:       0 warnings / 0 errors
C# tests:            142 / 142 PASS
frontend tests:      13 / 13 PASS
tool pack/install:   PASS
WorkPlay:            PASS
PokeTrade:           PASS
```

## Current V0.4.7-D focus

R7.10 now owns the only remaining D implementation question:

> For the same R7.9-proven item/dataflow path, what backend selection/eligibility conditions and frontend visibility/filter conditions jointly determine whether it is visible?

Target composition:

```text
R7.9-proven API → frontend item/value identity
+
backend business-condition evidence
+
frontend visibility/filter evidence
→ PO-facing joint visibility explanation
```

Required properties:

- reuse exact R7.9 item/dataflow identity; do not join predicates by name/text similarity;
- keep backend business-condition authority separate from frontend visibility evidence;
- never let frontend evidence upgrade observed-only/lower-authority backend evidence;
- fail closed on unrelated or ambiguous predicate/item paths;
- retain accepted R7.9 lineage and independent evidence when joint visibility cannot be proven.

E and V0.5 remain locked.

## V0.4.7 acceptance questions

The milestone is building toward portable answers for:

```text
Where did this value originally come from?
If the upstream value changes later, does the existing downstream value change automatically?
What code path can change this value after creation?
Was this value directly copied or computed?
What was the last observed source before a supported terminal boundary?
How did the value move through backend → DTO/projection → API → frontend composition?
What backend + frontend conditions jointly determine visibility?
If authority is incomplete, what deterministic lineage/causal evidence survives?
```

Before E can close, at least one unchanged real repository must naturally produce a useful positive V0.4.7 cross-layer answer from the generated knowledge pack. Benchmarks must not be modified merely to manufacture the supported shape.

## Knowledge hierarchy

```text
knowledge/index.md
  → observed capabilities and boundaries

knowledge/features/**/*.md
  → capability-level rules, permissions, outcomes and important failures

knowledge/workflows/**/*.md
  → operation-level behavior, validations, state changes, side effects, lineage and evidence

.pkc/facts.json
  → detailed implementation evidence and analyzer provenance
```

## Analyzer fidelity

Current analysis modes include strong C# project-semantic evidence plus explicitly bounded frontend and fallback modes. A high-confidence bounded frontend proof means the accepted grammar was matched exactly; it does not silently claim general TypeScript semantic resolution outside that boundary.

## Version semantics

Roadmap milestone, tool/package and serialized schema versions are independent.

```text
roadmap:             V0.4.7-D / R7.10 current
tool/package:        RuaDen.Pkc.Tool 0.4.3-preview.2
C# raw schema:       0.4.4-csharp-raw
merged facts schema: 0.4.4
cross-stack schema:  0.4.6
frontend schema:     0.4.3-frontend
```

Do not mechanically bump package or schema versions when a roadmap checkpoint advances.

## Quickstart from source

Until a new public package/release is explicitly accepted:

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

`pkc build <repository-path>` produces:

```text
.pkc/
  facts.json
  feature-candidates.json
  product-features.json

knowledge/
  AI_INSTRUCTIONS.md
  index.md
  features/
  workflows/

PKC_KNOWLEDGE.md
PKC_KNOWLEDGE.zip
```

Recommended usage:

```text
single-file AI upload
→ PKC_KNOWLEDGE.md

workspace supporting multiple files
→ knowledge/ starting from AI_INSTRUCTIONS.md and index.md

archive/storage
→ PKC_KNOWLEDGE.zip
```

The ZIP is a transport convenience. It contains portable knowledge by default, not raw `.pkc/facts.json` or source code.

Generated knowledge is currently `code-observed`. Requirement intent and delivery history remain unavailable until their evidence sources are integrated.

## Validation strategy

```text
focused checkpoint fixtures
→ full C# / frontend tests as relevant
→ Release build
→ PokeTrade known-answer
→ Loren pinned
→ Loren-main canary
→ Jellyfin pinned
→ portable parity/no-leak
→ independent rereview
```

CI is the final clean-environment layer, not the normal edit/test loop.

## Planned, not implemented yet

- D/R7.10: joint backend/frontend visibility on the same proven item path;
- E: final knowledge-only V0.4.7 acceptance plus unchanged-real-project positive yield;
- TypeScript `Program` / `TypeChecker` semantic analysis unless required by a proven gap;
- React AST-backed analysis unless required by a proven gap;
- full Angular template AST/compiler analysis unless required by a proven gap;
- additional frontend/backend framework adapters;
- Azure DevOps Epic / Feature / PBI / Sprint evidence with an explicit intent/history authority model;
- incremental compilation and PR knowledge diffs;
- cross-repository/system composition;
- runtime UI confirmation;
- product gap/drift analysis;
- stable query/MCP projections over the canonical portable knowledge model;
- optional LLM-assisted synthesis where deterministic grouping is insufficient.

A new analyzer capability enters a milestone only when a concrete acceptance regression proves it is needed.