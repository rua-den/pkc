# PKC — Product Knowledge Compiler

PKC turns implementation evidence into portable product knowledge that a Product Owner can attach to ChatGPT, Claude, Gemini, Copilot or another capable AI.

## Current working proof

The C# backend pipeline is now end-to-end:

```text
C# source
  ↓
Roslyn evidence
  ↓
.pkc/facts.json
  ↓
feature candidates
  ↓
.pkc/feature-candidates.json
  ↓
grounded knowledge model
  ↓
knowledge/features/**/*.md
```

`scan` stops at compact machine evidence:

```bash
dotnet run --project src/Pkc.Cli/Pkc.Cli.csproj -- scan <repository-path>
```

`build` produces the portable Markdown knowledge pack:

```bash
dotnet run --project src/Pkc.Cli/Pkc.Cli.csproj -- build <repository-path>
```

For the sample:

```text
samples/WorkPlaySample/knowledge/features/workplay/complete.md
```

The generated Markdown is intentionally marked `authority: code-observed`. Frontend/UI paths, Azure DevOps history and business approval are not invented when those sources have not been analyzed.

See `docs/vision.md`, `docs/architecture.md`, `docs/status.md`, `docs/handoff.md` and `docs/milestones.md`.
