# RD8 Private Large-Repository Validation — Result

Date: 2026-09-25
Checkpoint: V0.4.7-E0 / RD8
Candidate: `935ea94` (branch `codex/rd8-c-runtime-plugin`), PKC Release `net10.0`

Sanitized record. No target names, paths, source, identities or configuration values. Authoritative state remains `docs/status.md` / `docs/handoff.md`.

## Environment

- approved source-enabled company environment, local workstation
- target: disposable local clone of the approved private repository at its current HEAD, `.pkc` absent before the run
- the original checkout was only read, never written

## RD8-C — runtime/plugin real-target proof

```text
discovery (in-process, read-only, 935ea94)   runtime-plugin-load 2 HIGH, 1 production host
fresh CLI discover + run (935ea94)           runtime-plugin-load 2, project-reference 205
unresolved                                   project-file-not-found 1 (unrelated), no runtime-* reasons
```

Result: **PASS**. The applicable topology (folder-scan loader + target-local item copy) is deterministically represented. Details: `docs/reviews/2026-09-25-rd8-c0-target-classification.md`.

## RD8-A — fresh current-candidate operability

Method: `dotnet pkc.dll discover <copy>` then `dotnet pkc.dll run <copy>`, no `--resume`. Peak memory is the Win32 process-tree working-set sum, sampled every 2 s.

```text
discover   exit 0   1.2 min    peak 0.07 GB
run        exit 0   31.1 min   peak 5.42 GB   (run-summary total 1,862 s)

phase timings (s)   discover 1.9 / scan 1,729.7 / write evidence 2.8 / link 121.0 /
                    synthesize 1.8 / knowledge 0.8 / workspace 3.9

repository files 25,355 / hosts 18 / owned components 46 / test projects 7 / unknown-ownership components 2
planned semantic files 16,712 / executed 16,709 / withheld by scanner scope 3
test-evidence 1,982 / indexed 41 / not analyzable 6,620 / excluded areas 2 / unknown areas 0

facts 265,120 / relations 1,206,340 / workflow candidates 4,186 / product features 703
knowledge files 4,702 / workspace files 4,711 at .pkc/workspace
grounding: rules 3,049 / permissions 621 / state changes 2,423 / side effects 4 / UI steps 0 / UI->backend 13

artifacts: facts, checkpoint, workflow candidates, product features, AI workspace — all written
workspace no-leak spot check: 0 files with C# declaration markers, 0 with credential-shaped strings
```

Compared with the historical pre-integration run (~29 min / ~7.5 GB, OOM-free): elapsed is equivalent and peak memory is ~28% lower. Workflow candidates (4,186) and product features (703) match exactly.

Relation-count check (bounded, completed):

- A fresh baseline run of `main` (`ac1b829`) on the same copy is **identical** to `935ea94`: facts 265,120, relations 1,206,340, 16,712 planned files, and zero differing fact/relation kinds. Baseline run: exit 0, 28.3 min, peak 6.23 GB. The RD8-C commit therefore does not change semantic output.
- The comparison point is the previous local run from 2026-09-24. That run was a `--resume` of an earlier checkpoint built by an unrecorded binary. It had ~2.07M relations and ~268k facts.
- The whole difference sits in two places. `invokes` fell from ~956k to ~107k. `applies-mapped-field-rule` fell from ~4.3k to **0**, and business/observed predicates fell ~4k each. At the same time `ui-api-call` rose ~1.3k and `loads-api` ~0.5k.
- `applies-mapped-field-rule` is emitted by current production (`src/Pkc.CSharp/CSharpRuleSurfaceAnalyzer.cs:24`, introduced by `48ce2f1`), yet it is 0 on this real target at current main.

This is a **pre-existing current-main question, not caused by RD8-C**. It is carried as open item RD8-OBS-1. Either the mapped-field rule surface is not reached on the real target at current main, or the earlier checkpoint came from different code. A bounded investigation is required before the quantity-adjustment probe (#7) is re-scored after repairs, because `48ce2f1` was motivated by that probe.

Result: **PASS (operability)**. RD8-OBS-1 is carried open.

## RD8-B — targeted Level-1 product value

Protocol: `docs/benchmarks/product-value-benchmark-protocol.md`.
- Phase 1 was answered by isolated agents allowed to read only the generated `.pkc/workspace` of the `935ea94` run.
- Phase 2 was cross-checked by separate agents against the read-only original checkout in the approved environment.
- Detailed source-known answers stay in the approved environment. Below are sanitized behaviour-level results only.

Questions come from `docs/question-trainning.md`:
- #7 is the mandatory quantity-adjustment / scheduled-update rerun;
- #1 is a condition/temporal probe (lost date vs customer-portal access);
- #10 is a workflow/side-effect probe (work-log creation, content and email).

| Probe | Selected dimensions | Score | Label | Calibration |
| --- | --- | --- | --- | --- |
| #7 scheduled updates vs invoice period | Q2 state/effect | ~27% | FAIL | no wrong proven behaviour; one wrong explanation ("logic in DB procedures") |
| #1 lost date vs portal login | Q1 perm+precond ~15%, Q2 ~60% | ~38% | FAIL | **blocker**: two exclusive deactivation branches merged into one "proven" effect |
| #10 work log | Q2 ~45%, Q1 ~80% | ~55% | PARTIAL | good; two minor overstatements (number padding and search parsing, each generalised from a single site) |

Useful and correct where answered:
- save-time queueing and the full validation / edit / delete guard set (#7);
- lost date alone does not change customer flags (#1);
- work-log number formula, content filters and the manual resend flow (#10).

Every unanswerable part was honestly marked NOT PROVEN.

### Material misses — shared root causes

1. **Deferred command-queue indirection (#7, #10).** Producers insert a queue row that names its handler by type-name string, and a dispatcher runs the handler later. PKC does not link producer to handler. As a result the immediate scheduled-update processing, the automatic work-log email and its default recipients are all invisible.
2. **Recurring background jobs as triggers (#7, #10).** Scheduler-registered recurring job services are not surfaced as workflow triggers. This hides the scheduled-update processor, the hour-invoicing run that creates work logs, and their filter conditions.
3. **Entity creation through static factories and bulk insert (#10).** A static `Entity.Create(...)` plus bulk insert, with no repository add or unit-of-work save, is not recognised as an entity-create effect. Field lineage inside the factory (billable vs worked hours) is lost with it.
4. **Authentication-event access gates in a separate host (#1).** The portal's access rule lives in a token-validated event handler that calls a domain service whose conditions are query filters. It is not a controller attribute, so no permission or precondition is surfaced.
5. **Exclusive-branch merge (#1, calibration blocker).** Mutually exclusive if/else state effects in one method were rendered as one combined "proven" effect.
6. **Static processors reached only from invoicing services (#7).** The invoicing-time proration logic sits in a static processor. It is not tied to the scheduled-update entity or workflow.

Supporting guard gap: the workspace attributed missing logic to "database procedures" without finding a procedure that references the entity.

### RD8-B verdict

**NOT PASS.** One calibration blocker (exclusive-branch merge), plus FAIL on two of three probes. By the E0 completion rule, RD8 and E0 cannot close on this candidate.

Highest-ROI next repairs, as proposals for a separate regression-first task:
- **(a)** fix the exclusive-branch merge. It is a correctness blocker.
- **(b)** link deferred command-queue producers to their handlers. This lifts #7 and #10.
- **(c)** surface recurring jobs as triggers.

Items 3, 4 and 6 follow.
