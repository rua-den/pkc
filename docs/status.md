# PKC Status

Last updated: 2026-09-11

## Current milestone

**V0.1 baseline — C# evidence compiler: COMPLETE**

Verified code commit: `14045bf6fc723d82c8897cc0f7db8185582c9baf`

GitHub Actions run `34573299943` passed all checks:

- `dotnet build PKC.sln --configuration Release`
- unit tests
- end-to-end `pkc scan samples/WorkPlaySample`
- generated `.pkc/facts.json` contains the expected endpoint evidence

## What exists now

- `PKC.sln`
- `Pkc.Core` — evidence model
- `Pkc.CSharp` — Roslyn-based C# scanner
- `Pkc.Cli` — `pkc scan <repository-path>`
- `Pkc.CSharp.Tests`
- `samples/WorkPlaySample`
- GitHub Actions CI

## Evidence currently extracted

- class
- interface
- struct
- record
- enum and enum members
- methods and constructors
- properties
- attributes
- HTTP endpoint method + route template
- authorization presence/details
- syntactic method invocation relations
- exact source file and line ranges

All output is deterministic and written to:

```text
.pkc/facts.json
```

## Deliberate V0.1 limitations

Not implemented yet:

- semantic symbol resolution / true call graph
- conditions and validation rules as first-class facts
- assignments/state mutations
- throws/exceptions as behavior evidence
- event publication / integrations as first-class facts
- combined controller + action route resolution
- frontend/UI analysis
- Azure DevOps ingestion
- LLM synthesis
- Markdown product knowledge generation
- incremental hashing/rebuild

## Next engineering target

Before introducing an LLM, make the evidence layer rich enough to describe behavior.

Recommended next slice: **V0.1.1 — behavior evidence**

Extract with Roslyn:

1. `if`/guard conditions
2. thrown exceptions
3. assignments and state mutations
4. resolved method-call targets where possible
5. controller/action routes and authorization policies
6. event/message publication patterns

The output should remain source-grounded and deterministic.
