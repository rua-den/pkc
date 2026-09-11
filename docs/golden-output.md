# Generated Output Maintenance

PKC treats source evidence and the canonical knowledge model as the implementation truth. Generated Markdown is a product of the compiler, not a second source of truth.

## Current CI policy

The current CI does **semantic output assertions** rather than a full-repository golden snapshot:

- required output files must exist;
- important workflow text such as UI actions and permissions must appear;
- the PokeTrade benchmark must run its real business flow before PKC compiles knowledge from the same source tree.

This keeps formatting-only changes from creating unnecessary snapshot churn while still protecting important behavior.

## When to use a golden file

Use a committed golden Markdown file only when exact formatting or a stable public contract is intentionally under test. A golden file should represent one small scenario, not the whole generated knowledge directory.

## Updating a golden file

When compiler behavior intentionally changes:

1. run `pkc build <sample>`;
2. inspect the evidence and generated Markdown manually;
3. verify the change is caused by the intended source behavior, not parser noise;
4. update the golden file only after the new output is accepted;
5. keep semantic assertions for critical business facts even when a golden snapshot exists.

Do not update a golden file merely to make CI green. If unexpected facts appear, fix the analyzer/linker first.

## PokeTrade benchmark

`samples/PokeTradeSystem` is the main real-system benchmark. CI validates the runnable Order → WorkPlay → Delivery flow and then runs PKC against that source. Knowledge correctness should be reviewed against the working application before expanding scope.
