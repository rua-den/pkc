# PKC Status

Last updated: 2026-09-11

## Current milestone

**V0.4.3 Analyzer Fidelity Hardening — COMPLETE**

Verified implementation/acceptance commit: `8f69d6c931e917ce7538b9a991a05211e624cb45`

Verified GitHub Actions run: `34627169975` (#85)

Both jobs are green:

- `test`: solution build, unit/regression tests, `0.4.3-preview.1` local .NET tool pack/install, WorkPlay end-to-end knowledge build
- `poketrade-real-system`: .NET 10 backend build, Angular 22 frontend build, runtime branch acceptance, analyzer-fidelity contract and product-knowledge contract

## What V0.4.3 changes

### C# backend

PKC now prefers the target project's actual compilation context:

```text
.csproj
  ↓
MSBuildWorkspace
  ↓
target project references / framework references / compilation
  ↓
Roslyn SemanticModel
```

Facts loaded through this path are tagged:

```text
analysisMode: project-semantic
analysisConfidence: high
semanticContext: target-project
```

PokeTrade acceptance proves semantic resolution of `Microsoft.AspNetCore.Mvc.ControllerBase` and HTTP method attributes. Semantic call relations for project-backed methods are re-enriched from the target project SemanticModel.

If a project cannot be loaded/mapped, PKC keeps the existing loose Roslyn analysis but explicitly tags it as:

```text
analysisMode: loose-roslyn-fallback
analysisConfidence: medium
semanticContext: runtime-platform-assemblies-only
```

Fallback is allowed; silent fallback is not.

### Angular frontend

Angular TypeScript structure now prefers the target project's local TypeScript parser/AST for:

- component classes
- application routes
- method structure needed by page-load flows
- HTTP call expressions

These facts are tagged:

```text
analysisMode: typescript-ast
analysisConfidence: high
```

Angular template action/visibility extraction is **not AST-backed yet**. It remains a conservative fallback and is tagged:

```text
analysisMode: angular-template-regex-fallback
analysisConfidence: medium
```

If the Angular TypeScript runtime/Node AST path is unavailable, the adapter can fall back to the legacy text scanner with:

```text
analysisMode: regex-fallback
analysisConfidence: low
```

### React frontend

React is still the existing conservative regex/text adapter. Its evidence is now explicitly tagged `regex-fallback` / `low` instead of being presented with ambiguous fidelity.

### Knowledge boundary

When fallback evidence contributes to a workflow, the candidate/Markdown carries an explicit `Important unknowns` warning. Downstream synthesis must not silently promote fallback evidence to high-confidence claims.

## Acceptance evidence

The PokeTrade benchmark was deliberately changed without changing behavior so that the previous Angular regex-only implementation would miss important facts:

- `createOrder` uses a multiline/chained `HttpClient.post(...)` call;
- the `/orders` route uses quoted object-property keys.

Run #85 still produces the expected knowledge and additionally asserts:

```text
project-semantic
semanticContext = target-project
semanticBaseType = Microsoft.AspNetCore.Mvc.ControllerBase
endpointAttributeResolution = semantic
typescript-ast
angular-template-regex-fallback
```

and rejects a full Angular `regex-fallback` path for this benchmark.

The packaged CLI also scans WorkPlay through `project-semantic` in CI.

## What “verified” means

“Verified” means verified against the current WorkPlay and PokeTrade acceptance systems. It does **not** mean PKC has already proven robustness across arbitrary real-world repository styles.

That is the next milestone.

## Current commands

```bash
pkc scan <repository-path>
pkc build <repository-path>
```

Current local tool package version:

```text
RuaDen.Pkc.Tool 0.4.3-preview.1
```

Outputs:

```text
.pkc/facts.json
.pkc/feature-candidates.json
.pkc/product-features.json
knowledge/index.md
knowledge/features/**/*.md
knowledge/workflows/**/*.md
```

## Known boundaries — explicit, not hidden

- Angular TypeScript is AST-backed, but Angular template actions remain a regex/template fallback.
- React remains regex fallback; React AST is not part of V0.4.3.
- Other frontend frameworks are not implemented yet.
- Azure DevOps intent/history is not compiled yet.
- runtime browser/UI exploration is not implemented yet.
- product intent is separate from code-observed implementation.
- project load can still fail on unusual/build-environment-dependent C# repositories; this must surface as fallback provenance rather than be hidden.
- minimal API endpoints such as `/health` remain outside the current MVC endpoint path.

## Next target

**V0.4.4 — external real-project trial.**

Run the packaged V0.4.3 tool against one genuine repository and classify findings as:

```text
wrong claim
missing important behavior
noise
unsupported stack/pattern
unexpected fallback
```

Fix only gaps proven by the external repository and add a regression fixture for each fix.

Do not start V0.5 Azure DevOps yet.
