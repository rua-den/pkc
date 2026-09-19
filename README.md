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
V0.4.7-D API → UI composition              CURRENT / UNLOCKED
V0.4.7-E product acceptance                LOCKED behind D
V0.5 Azure DevOps input evidence           LOCKED
```

Accepted V0.4.6 production:

```text
c310e893762997f34562a6b3a62dbab2b05c0c93
fix: fail closed on Queryable authority
```

Accepted final V0.4.7-C production code:

```text
fbb64b9917da1f63362558355201ff7998384ba0
feat: prove backend API projection lineage
```

V0.4.7-D is current. Azure DevOps ingestion remains locked until V0.4.7 and the V0.4.x PO-question-readiness exit gate pass.

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
- Angular TypeScript structure/routes/HTTP-call shapes through the project-local TypeScript syntactic AST when available;
- Angular template actions through explicit conservative fallback;
- React/TypeScript through conservative fallback;
- framework-agnostic frontend adapter boundary (`IFrontendAdapter`);
- UI action → API call → backend endpoint linkage;
- workflow Markdown, product-feature Markdown and `knowledge/index.md`;
- portable `PKC_KNOWLEDGE.md` and `PKC_KNOWLEDGE.zip` handoff artifacts;
- PokeTrade known-answer runnable regression;
- Loren pinned real-project acceptance plus Loren-main moving canary;
- Jellyfin pinned independent generalization with portable parity/no-leak verification.

### Accepted V0.4.7 value-lineage scope through C

PKC additionally supports bounded target-project-semantic evidence for:

```text
stored direct copy
stored snapshot timing
reference/dynamic read-time dependency
multi-input scalar derivation
later supported override / mutation causality
last proven source before a supported direct return
backend entity/domain property → explicit DTO/response property → API response
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
```

A stored derivation is a point-in-time snapshot. Later changes to its inputs do not rewrite the stored value without another proven mutation.

C's DTO/API edge is based on exact target-project semantic assignment and project/assembly/type/member identity. Renamed DTO properties are supported when assignment proves the mapping. Same/equivalent names alone never create lineage.

### Fail-closed behavior

Permanent accepted regressions through C include:

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
- custom source/target DTO accessors and unsupported projection effects.

Unsupported shapes fail closed while independently proven lower-authority evidence remains available.

This is deliberately **not** a general symbolic execution, alias, effect, arbitrary helper-projection or persistence solver.

## Final V0.4.7-C verification

Exact-main gates on `fbb64b9917da1f63362558355201ff7998384ba0`:

```text
CI + full PKC tests + WorkPlay + PokeTrade   35416169067 — PASS
pinned Loren                                35416169091 — PASS
Loren-main canary                           35416169051 — PASS
pinned Jellyfin                             35416169039 — PASS
```

Core:

```text
Release build:       0 warnings / 0 errors
C# tests:            136 / 136 PASS
frontend tests:      13 / 13 PASS
tool pack/install:   PASS
WorkPlay:            PASS
PokeTrade:           PASS
```

Pinned Jellyfin:

```text
facts:                   43,365
relations:               195,316
workflow candidates:     386
product features:        116
canonical Markdown:      504
project-semantic facts:  43,365 / 43,365
portable bundle parity:  PASS
ZIP parity:              PASS
raw .pkc/src leak:       none
artifact id:             10575663250
artifact digest:         sha256:857d33020332f4a68a69177b6809136ebe24b9ea86b5576efc8dbc69d8266347
artifact size:           9,162,486 bytes
```

Final C review: `docs/reviews/2026-09-19-v0.4.7-c-final-rereview.md`.

## Current V0.4.7-D focus

D answers:

> What frontend state/display does this proven API result feed, and what backend + frontend conditions jointly determine visibility for the same proven item path?

The first D checkpoint is regression-first and bounded. It must prove an equivalent of:

```text
API response field
→ frontend HTTP/API result
→ component/view-model assignment
→ rendered/displayed value
```

Required properties:

- exact backend response identity and frontend result/receiver/assignment identity;
- backend and frontend source locations;
- no normalized-name, route-label, casing or property-name join;
- candidate retention → synthesis → PO-facing workflow Markdown;
- fail closed when receiver/result/binding identity is ambiguous or unsupported;
- retain accepted backend lineage when frontend composition cannot be proven.

Joint backend/frontend visibility composition follows only after the same item/dataflow path is proven. Business-condition authority and frontend visibility evidence remain separate.

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

Current analysis modes include:

```text
project-semantic
  C# target-project MSBuildWorkspace context; strongest supported C# authority.

typescript-ast-syntactic
  Angular project-local TypeScript parser without TypeChecker receiver proof.

loose-roslyn-fallback
  C# source analyzed without full target-project reference graph.

angular-template-regex-fallback
  conservative Angular template action/visibility extraction.

regex-fallback
  low-authority conservative text fallback, including currently supported React shapes.
```

D must not pretend a syntax-only frontend fact has semantic identity it does not actually prove.

## Version semantics

Roadmap milestone, tool/package and serialized schema versions are independent.

```text
roadmap:             V0.4.7-D current
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

- D: API result → frontend binding/composition and joint backend/frontend visibility;
- E: final knowledge-only V0.4.7 acceptance;
- TypeScript `Program` / `TypeChecker` semantic analysis unless required by a proven D gap;
- React AST-backed analysis unless required by a proven gap;
- full Angular template AST/compiler analysis unless required by a proven gap;
- additional frontend/backend framework adapters;
- Azure DevOps Epic / Feature / PBI / Sprint evidence;
- incremental compilation and PR knowledge diffs;
- runtime UI confirmation;
- product gap/drift analysis;
- optional LLM-assisted synthesis where deterministic grouping is insufficient;
- optional live delivery adapters after the portable knowledge contract is proven.

A new analyzer capability enters a milestone only when a concrete acceptance regression proves it is needed.

## Project status

Authoritative project files:

- `docs/status.md`
- `docs/handoff.md`
- `docs/milestones.md`
- `docs/v0.4.7-acceptance-plan.md`
- `docs/product-knowledge-contract.md`
- `docs/vision.md`
- `docs/real-project-trial.md`
- `docs/ai-handoff.md`
