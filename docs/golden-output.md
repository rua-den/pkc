# Generated Output Maintenance

PKC treats source evidence and the canonical knowledge model as the implementation truth. Generated Markdown is a product of the compiler, not a second source of truth.

## Output acceptance has two jobs

Generated output must satisfy both:

```text
1. factual grounding / traceability
2. usable product comprehension
```

A knowledge file can be factually correct and still be a bad PKC output if helper-level implementation detail overwhelms the product meaning.

## Current CI policy

CI uses semantic output assertions rather than a full-repository golden snapshot:

- required output files must exist;
- critical workflow behavior/permissions/responses must appear;
- benchmark-specific wrong claims/noise must be rejected;
- PokeTrade runs real business branches before knowledge compilation;
- the pinned Loren external trial builds the real repository and asserts important real-repo behavior/provenance.

These assertions protect known bugs and product-critical facts, but **CI grep success alone is not final knowledge acceptance**.

## Knowledge-only comprehension review

For an external real-project milestone, manually review the generated pack as an actual consumer would:

1. hide the source repository;
2. start from `knowledge/index.md`;
3. read feature files first;
4. open workflow files only when the question needs detail;
5. answer a fixed set of product/system questions using only `knowledge/`;
6. compare those answers against source/known behavior afterward.

Classify failures as:

```text
wrong claim
missing important behavior
product-level noise / wrong abstraction
unsupported required pattern
unexpected fallback
unclear or hidden uncertainty
```

This review tests the actual PKC product contract: portable knowledge that lets a PO/AI understand the system without rescanning source.

## Abstraction expectations

### Feature/index layer

Should prioritize:

- product capabilities;
- meaningful user/system operations;
- important permissions;
- business/product rules;
- important failures and caveats;
- links to detailed workflows.

It should not mechanically dump all transitive helper guards, loops, string-processing branches or framework plumbing.

### Workflow layer

May contain deeper implementation-observed behavior when it is useful to explain the operation or provide traceability, but should remain application-oriented.

### Evidence layer

May retain detailed facts and relations even when they are intentionally omitted from feature/workflow prose. Presentation filtering must not destroy raw evidence merely to make Markdown cleaner.

## When to use a golden file

Use a committed golden Markdown file only when exact formatting or a stable public contract is intentionally under test. A golden file should represent one small scenario, not the whole generated knowledge directory.

## Updating a golden file

When compiler behavior intentionally changes:

1. run `pkc build <sample-or-trial-repo>`;
2. inspect evidence and generated Markdown manually;
3. verify the change is caused by intended source behavior, not parser noise;
4. verify product abstraction did not regress while fixing correctness;
5. update the golden file only after the new output is accepted;
6. keep semantic assertions for critical business facts even when a snapshot exists.

Do not update a golden file merely to make CI green. If unexpected facts appear, fix the analyzer/linker/synthesizer at the correct layer.

## Benchmarks

### PokeTrade

`samples/PokeTradeSystem` is the known-answer runnable business benchmark. CI validates the Order → WorkPlay → Delivery flow and then checks generated knowledge against the working application.

### Loren

`rua-den/loren` is the current external real-project trial. It exists to expose real repository patterns and, more importantly, to test whether the generated `knowledge/` is actually understandable as product/system knowledge rather than only technically traceable source evidence.
