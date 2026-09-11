# PKC Handoff

Use this file when continuing PKC in another chat/session.

## Product idea

PKC means **Product/System Knowledge Compiler**.

The target user is a Product Owner. A PO should be able to take PKC's generated `knowledge/` folder and attach it to any capable AI — ChatGPT, Claude, Gemini, Copilot, etc. — then ask questions about the product without making that AI re-read or grep the source repository.

Example target question:

> How do I change the status of a WorkPlay? What should I be careful about?

The mature knowledge pack should combine:

- how users perform actions in the UI
- business behavior and status transitions
- validations and permissions
- side effects and integrations
- Feature/PBI/Sprint history
- current delivery state
- gaps and improvement opportunities
- evidence supporting important claims

## Non-negotiable compiler architecture

Do not implement `source -> LLM -> Markdown` directly.

```text
SOURCE
  ↓
DETERMINISTIC ANALYZERS / ADAPTERS
  ↓
EVIDENCE / FACT MODEL
  ↓
FEATURE / WORKFLOW CANDIDATES
  ↓
KNOWLEDGE SYNTHESIS
  ↓
CANONICAL KNOWLEDGE MODEL
  ↓
DETERMINISTIC MARKDOWN RENDERER
```

## Frontend architecture decision

Frontend technology must stay outside the compiler core.

```text
React / Angular / future MVC-Razor / Blazor / Vue
                    ↓
              IFrontendAdapter
                    ↓
          canonical `ui-*` evidence
                    ↓
             FrontendScanner
                    ↓
          generic relation linker
                    ↓
          shared evidence pipeline
```

Current frontend adapters:

- React/TypeScript static
- Angular static

New frameworks must be adapters that emit the same canonical facts; do not add framework branches to feature/knowledge compilation.

## Current verified state

**V0.4.2 PokeTrade real-system knowledge benchmark is COMPLETE.**

Final acceptance code/contract commit: `b29bb0d6d7c56f0676dd0c9eddbe9faf8c9ddec7`

Final CI run: `34620984359`

Verified in the same run:

- PKC solution build
- all unit/regression tests
- local `.NET tool` pack/install and `pkc` execution
- WorkPlay end-to-end knowledge build
- PokeTrade .NET 10 backend build
- PokeTrade Angular 22 frontend build
- PokeTrade runtime branch acceptance
- generated product-knowledge contract assertions

PKC is cleared for **V0.4.3 external real-project trial**.

## PokeTrade benchmark coverage

The mini project was reviewed file-by-file across backend and frontend, then used to harden the compiler.

The acceptance suite locks representative behavior for:

```text
stock sufficient → reserve → ReadyForDelivery + Delivery
stock insufficient → AwaitingStock + PurchaseStock WorkPlay
PurchaseStock QuantityToBuy = shortage + reorder level
order validation failures
WorkPlay Open → InProgress → Completed
invalid WorkPlay transitions / invalid purchased quantity
missing WorkPlay → 404
complete purchase → inventory increase + waiting-order re-evaluation
waiting orders iterated in ascending order ID
Delivery Pending → Dispatched → Delivered
invalid Delivery transitions
missing Delivery → 404
```

## Compiler gaps fixed during the full benchmark review

The PokeTrade review found and closed these blockers:

1. business-important object construction was being suppressed as initializer noise;
2. computed domain properties such as `Order.Total` were missing;
3. waiting-order collection/loop semantics were under-described;
4. Angular UI action evidence lacked surrounding status guards;
5. two-hop component/helper → service → HTTP read flows were incomplete;
6. configured authorization policy definitions were missing;
7. passive page-load API flows were under-linked;
8. null-coalescing `?? throw` lookup behavior was not represented strongly enough;
9. controller 400 / 404 / 409 error-response mappings were not captured as knowledge.

These are now regression/acceptance-covered in the PokeTrade benchmark.

## Current generated knowledge

Commands:

```bash
pkc scan <repository-path>
pkc build <repository-path>
```

Main outputs:

```text
.pkc/facts.json
.pkc/feature-candidates.json
.pkc/product-features.json
knowledge/index.md
knowledge/features/**/*.md
knowledge/workflows/**/*.md
```

`knowledge/` is portable and intended to be consumable by any capable AI. Current authority remains `code-observed`; it is not business-approved intent.

## Known boundaries — not V0.4.2 blockers

- frontend static adapters currently cover React/TypeScript and Angular
- Azure DevOps delivery/history/intent is not compiled yet
- runtime browser/UI exploration is not implemented yet
- analyzers intentionally prefer explicit unknowns over guessing
- minimal API endpoints such as `/health` are outside the current MVC endpoint scanner and are operational rather than core benchmark knowledge

## Next engineering target — keep narrow

**V0.4.3 external real-project trial.**

Do not add speculative capability first. Run the current compiler against a real repository and let real missing/wrong knowledge drive the next fixes.

Recommended gate for V0.4.3:

```text
real project builds/runs normally
    ↓
pkc build <real-project>
    ↓
generated knowledge reviewed against source + known product behavior
    ↓
classify failures as parser / linker / grouping / synthesis / renderer gaps
    ↓
fix only proven gaps + add regression fixture
```

Do not start V0.5 Azure DevOps or incremental architecture work until the external trial is understood.
