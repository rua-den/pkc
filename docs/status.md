# PKC Status

Last updated: 2026-09-12

## Current milestone

**V0.4.3 Analyzer Fidelity Hardening — COMPLETE, post-review fixes included**

Verified implementation/acceptance commit: `5c457111d072ad5f7b93bf3cff49d27960ac79fb`

Verified GitHub Actions run: `34630303904` (#95)

Both jobs are green:

- `test`: solution build, unit/regression tests, `0.4.3-preview.2` local .NET tool pack/install, WorkPlay end-to-end knowledge build;
- `poketrade-real-system`: .NET 10 backend build, Angular 22 frontend build, runtime branch acceptance, analyzer-fidelity contract and product-knowledge contract.

## External-review findings closed in this checkpoint

A follow-up review confirmed the MSBuildWorkspace implementation, fallback propagation and CI path were real, then identified two fidelity-label gaps. They are now fixed.

### C# project loaded vs. declaration actually matched

`project-semantic` no longer implies that declaration-node matching always succeeded.

For declaration-like facts PKC now records:

```text
semanticNodeMatch: matched | failed
```

If the target project loads but PKC cannot match the original fact back to the corresponding syntax node:

```text
analysisMode: project-semantic
analysisConfidence: medium
semanticNodeMatch: failed
analysisCaveat: target-project-loaded-but-fact-node-match-failed
```

A regression test intentionally supplies a mismatched fact location and locks this behavior. Non-declaration evidence uses `semanticNodeMatch: not-applicable`.

### Angular TypeScript is syntactic AST, not type-checked semantics

The Angular scanner uses the target repo's local TypeScript parser with `ts.createSourceFile`. It does **not** yet create a TypeScript `Program` or use a `TypeChecker`.

The provenance label is therefore now:

```text
analysisMode: typescript-ast-syntactic
typescriptSemanticContext: syntax-only-no-type-checker
```

Confidence is split by evidence kind:

```text
ui-screen / ui-route   high
ui-api-call            medium
```

`ui-api-call` additionally carries:

```text
httpReceiverResolution: syntactic-unverified
analysisCaveat: http-method-name-and-url-shape-detected-without-receiver-type-checking
```

This avoids claiming that `.get/.post/.put/.patch/.delete` receiver types are proven to be Angular `HttpClient` when they are currently matched syntactically.

### Angular AST runtime precondition

The syntactic AST path currently requires:

- `node` available on `PATH`;
- a target-repository local `node_modules/typescript/lib/typescript.js`, normally after the repository dependency-install step (`npm install`, `npm ci`, `pnpm install`, etc.).

If unavailable, Angular explicitly falls back to `regex-fallback` / `low` with a recorded reason such as `local-typescript-runtime-not-found` or `node-unavailable: ...`.

## Acceptance evidence

Run #95 asserts all of the following against real generated `.pkc/facts.json`:

```text
project-semantic
semanticContext = target-project
semanticNodeMatch = matched
semanticBaseType = Microsoft.AspNetCore.Mvc.ControllerBase
endpointAttributeResolution = semantic
typescript-ast-syntactic
typescriptSemanticContext = syntax-only-no-type-checker
ui-api-call confidence = medium
httpReceiverResolution = syntactic-unverified
ui-screen/ui-route confidence = high
angular-template-regex-fallback
```

and rejects a full Angular `regex-fallback` path for PokeTrade.

The packaged CLI also scans WorkPlay through `project-semantic` and verifies at least one declaration `semanticNodeMatch=matched`.

## What “verified” means

“Verified” means verified against the current WorkPlay and PokeTrade acceptance systems. It does **not** mean PKC has already proven robustness across arbitrary real-world repository styles.

That remains the next milestone.

## Current commands

```bash
pkc scan <repository-path>
pkc build <repository-path>
```

Current local tool package version:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
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

- Angular TypeScript is syntactic-AST-backed, but `ui-api-call` receiver types are not type-checked yet.
- Angular template actions remain a regex/template fallback.
- The Angular AST path depends on Node.js and the target repo's installed local TypeScript runtime.
- React remains regex fallback; React AST is not part of V0.4.3.
- Other frontend frameworks are not implemented yet.
- Azure DevOps intent/history is not compiled yet.
- runtime browser/UI exploration is not implemented yet.
- product intent is separate from code-observed implementation.
- project load can still fail on unusual/build-environment-dependent C# repositories; this surfaces as explicit fallback provenance.
- declaration node matching can fail even after a project loads; this now surfaces as `semanticNodeMatch=failed` and reduced confidence.
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
