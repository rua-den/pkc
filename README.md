# PKC — Product Knowledge Compiler

PKC compiles source-code evidence into portable product knowledge that a Product Owner can attach to any capable AI.

## Current prototype

The current C# prototype uses Roslyn to build deterministic source evidence and then groups endpoint-centered evidence into compact feature candidates.

```text
C# source
  ↓
Roslyn evidence extraction
  ↓
.pkc/facts.json
  ↓
feature candidate grouping
  ↓
.pkc/feature-candidates.json
```

Run:

```bash
dotnet run --project src/Pkc.Cli/Pkc.Cli.csproj -- scan <repository-path>
```

Current output:

```text
<repository-path>/.pkc/facts.json
<repository-path>/.pkc/feature-candidates.json
```

The next milestone turns grounded feature candidates into the first portable Markdown knowledge file for AI testing.

See `docs/vision.md`, `docs/architecture.md`, `docs/status.md`, and `docs/handoff.md` for the project direction and current checkpoint.
