# PKC Handoff

Last updated: 2026-09-23

This handoff is for the next Claude Code / Opus coding or audit session.

## Read first

1. root `CLAUDE.md`
2. `AGENTS.md`
3. `docs/status.md`
4. this handoff
5. `docs/milestones.md`
6. `docs/product-knowledge-contract.md`
7. `docs/v0.4.7-acceptance-plan.md`
8. `docs/plans/2026-09-22-ai-workspace-continuous-update-plan.md`
9. `docs/reviews/2026-09-22-ai-workspace-continuous-update-plan-self-review.md`
10. `docs/benchmarks/product-value-benchmark-protocol.md`
11. `docs/benchmarks/2026-09-23-ai-question-answerability-benchmark.md`
12. `docs/reviews/2026-09-23-ai-workspace-preview-company-audit-request.md`
13. `docs/reviews/2026-09-23-v0.4.7-d-r7.10-rereview-17-request.md` only when working on the formal R7.10/D gate

Then inspect current `main`, recent commits, working tree state, production code and relevant regressions. The repository is the source of truth; do not reset to a historical SHA merely because this handoff names it.

## Keep formal acceptance and the user-authorized preview separate

Formal V0.4.7 state:

```text
A/B/C                    PASS / COMPLETE
R7.9                     PASS / COMPLETE
mutation-causality       PASS / CLOSED
R7.10                    REPAIRED / ALL GATES PASS / PENDING REREVIEW #17
V0.4.7-D                 PENDING INDEPENDENT REREVIEW #17
V0.4.7-E                 LOCKED
R7.14                    NOT PASS / REQUIRED FOR E
```

Formal R7.10 candidate:

```text
96205a9a643864facaf9642a3b390ddcdbed59d9
fix: tolerate duplicate Angular import aliases
```

Do not self-certify this candidate from its implementation continuation.

User-authorized AI workspace preview current exact source:

```text
c45eb24809f181d48eac53abc64e8b5e816c57cc
feat: isolate colocated PKC workspace from Git
```

This preview is validated for company-repository testing but does not mark formal W complete.

## Company repository UX

```text
pkc run <TEAM_REPOSITORY_PATH>
cd <TEAM_REPOSITORY_PATH>/.pkc/workspace
claude
```

Source-checkout equivalent:

```text
dotnet build PKC.sln --configuration Release
dotnet run --project src/Pkc.Cli/Pkc.Cli.csproj --configuration Release --no-build -- run <TEAM_REPOSITORY_PATH>
cd <TEAM_REPOSITORY_PATH>/.pkc/workspace
claude
```

The generated workspace lives beside the source under `.pkc/`; the user should not need to upload `PKC_KNOWLEDGE.md` for the preferred UX.

## Co-located workspace contract

```text
<repo>/.pkc/
  facts.json
  feature-candidates.json
  product-features.json
  workspace/
    CLAUDE.md
    AGENTS.md
    knowledge/
      START_HERE.md
      index.md
      features/...
      workflows/...
    _policy/
      answer-contract.md
    _meta/
      manifest.json
      catalog.json
      git-isolation.json
      source-context.json
```

Same repository root does not mean same authority.

### PRODUCT — default

- read generated workspace only;
- do not climb to `../..` source;
- answer in business/QA language;
- prefer unknown/not-grounded over guessing;
- do not dump source, raw facts, secrets or proprietary file bodies.

### TRACE — explicit

- remain workspace-only;
- may cite generated evidence paths, symbols and endpoints;
- do not open or reproduce source bodies.

### ENGINEERING — explicit

- source root is declared by `_meta/source-context.json` as `../..` relative to the workspace;
- source inspection/editing is allowed only in an approved company/source-enabled Claude Code environment;
- do not export source bodies into generated workspace, PKC public docs, issues or benchmark reports.

### Benchmark

Phase 1 must answer from generated workspace only and record the answer. Phase 2 may inspect the minimum required source locally in the approved company environment to establish the known answer and score the workspace answer. Reports contain behavior, evidence locations/symbols, scores and misses, not proprietary code bodies.

## Git isolation added by `c45eb248...`

`pkc run` now attempts to keep generated `.pkc/` out of ordinary `git add .` without modifying the team's tracked ignore policy.

- normal checkout: use `.git/info/exclude`;
- worktree/separate gitdir: follow `.git` `gitdir:` plus `commondir` and use common Git `info/exclude`;
- add `/.pkc/` idempotently;
- never edit tracked `.gitignore`;
- write result to `.pkc/workspace/_meta/git-isolation.json`;
- non-Git, unsupported or read-only layouts do not fail workspace generation.

Important limitation: ignore rules do not untrack already-tracked `.pkc` files. PKC deliberately does not rewrite the target repository index/history.

After running on a team repo, check:

```text
git status --short
```

New `.pkc/` output should be absent from status for a supported Git layout when it was not previously tracked. If not, inspect `_meta/git-isolation.json` before changing repository settings manually.

## Exact validation

Exact source:

```text
c45eb24809f181d48eac53abc64e8b5e816c57cc
```

Exact-SHA gates:

```text
CI / full tests / WorkPlay / PokeTrade  35852404008  PASS
pinned Loren                            35852403984  PASS
Loren-main canary                       35852404005  PASS
pinned Jellyfin                         35852404020  PASS
```

Observed exact results:

```text
Release build       0 warnings / 0 errors
C# tests            265 / 265 PASS
Frontend tests      13 / 13 PASS
Tool pack/install   PASS
WorkPlay            PASS
PokeTrade           PASS
Loren pinned/main   PASS
Jellyfin parity     PASS
```

Focused regression:

`tests/Pkc.CSharp.Tests/PkcWorkspaceColocationTests.cs`

This change is benchmark Level 0 because it changes workspace placement/Git isolation metadata, not product-answer semantics. Do not spend a full AI corpus on it.

## Benchmark cadence

Protocol:

`docs/benchmarks/product-value-benchmark-protocol.md`

```text
Level 0 — deterministic default
Level 1 — targeted AI only when product answers can change
Level 2 — full AI for acceptance/release/demo checkpoints, major semantic/routing changes, broad regression risk, or explicit request
```

For Level 1/2 always capture phase-1 workspace answer before inspecting source.

## Product-value baseline

```text
Agentic Users Update       80.8%
Jin12 Contacts Update      40.0%
Kesetovic PackOrder        65.0%
backend PO/QC core         72.6%
overall applicable         63.9%
```

Safety/fail-closed PASS does not mean product-value PASS.

## Next semantic coding priority — DeepSeek review reconciled with current architecture

Do not attack all three fixes in one patch. Use regression-first and targeted Level-1 benchmark after each semantic checkpoint.

### Fix #1 — interface → concrete implementation traversal

This is the next highest-ROI semantic repair and should start first.

Required proof shape:

```text
endpoint/controller invocation
→ exact interface method symbol
→ proven DI registration
→ exact concrete implementing method
→ concrete guards / mutations / downstream calls already present in evidence
```

Rules:

- use Roslyn/project-semantic identities, not method-name matching;
- scan registration sites across the loaded compilation, including top-level `Program.cs`;
- first bounded authoritative scope: direct `AddScoped<I,T>`, `AddTransient<I,T>`, `AddSingleton<I,T>`;
- resolve the exact interface member implementation, including overload identity/signature;
- retain interface-call evidence and add an explicit dispatch/resolution proof edge rather than pretending the interface call was originally a concrete call;
- multiple/ambiguous implementations must downgrade to uncertain/not grounded;
- factory delegates, assembly scanning, decorators, keyed and conditional registrations remain unsupported until deterministic proof and regressions exist;
- candidate traversal must follow the proven dispatch edge without weakening current fail-closed authority;
- targeted benchmark: Jin12 Contacts Update/GetAll Q1/Q2/Q4, then source cross-check.

Expected score increases are hypotheses, not acceptance criteria. Acceptance is proof correctness + safety preservation.

### Fix #2 — frontend URL expression resolution

After #1 closes, build a bounded TypeScript AST expression evaluator rather than widening regexes. Target literals, binary `+`, template spans, local constants and proven `this.property` values. Normalize route parameters and match backend route templates. Run targeted Kesetovic Q3/Q4.

If URL matching is fixed but PackOrder still lacks action→API proof, investigate the real child `@Output` → parent event handler bridge as a separate regression. Do not assume URL folding alone solves the full UI event chain. HttpParams/HttpHeaders are not part of the first route-matching repair unless a targeted regression requires them.

### Fix #3 — displayed-value lineage

Extend the existing bounded lineage model. First known target:

```text
MAT_DIALOG_DATA
→ this.data.email
→ FormControl/FormBuilder initialization
→ form control `email`
→ formControlName="email"
→ displayed field
```

Every promoted hop needs exact source location/proof identity and ambiguity handling. Reuse current `ui-field`, form behavior and rendered-value lineage facts where possible; do not start with an unbounded generic dataflow engine. Targeted benchmark: Agentic Users Update Q3/Q4.

Portable evidence/report format should use path + line range + symbol/fact/proof/confidence + business description. Do not persist proprietary raw source snippets in the workspace or reports.

After all three semantic checkpoints are independently verified, run a Level-2 three-repository benchmark and compare against the exact 63.9% baseline, plus report newly discovered unsupported cases.

## Privacy boundary for company Claude / Opus

- Proprietary target source stays in the company-approved Claude Code/enterprise environment.
- Normal PO/QA sessions should launch from `.pkc/workspace` and remain there.
- ENGINEERING and benchmark phase 2 may inspect source because it is co-located, but only after explicit mode/phase transition.
- PKC generated output must not contain source-code bodies or secrets.
- PKC cannot guarantee provider/network retention; organizational controls remain required.

## Terminal state / discipline

Keep moving on the assigned checkpoint until one of:

1. PASS and verified;
2. a required external review/gate cannot be performed in-session;
3. a genuinely external blocker is proven and documented.

Test/build/tool failure is evidence to investigate, not a reason to stop. Keep one coherent implementation commit/push where possible, then one docs-only handoff commit when needed.
