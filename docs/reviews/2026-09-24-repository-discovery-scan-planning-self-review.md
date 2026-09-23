# Repository Discovery + Scan Planning Plan — Self Review

Date: 2026-09-24
Review target: `docs/plans/2026-09-24-repository-discovery-scan-planning-plan.md`
Decision: READY AS A PREPARED / LOCKED PLAN; NOT AUTHORIZED FOR PRODUCTION IMPLEMENTATION YET

## Review question

Does the plan convert the sanitized large-legacy-repository reconnaissance into a generic PKC design without leaking proprietary source, hard-coding one repository, weakening fail-closed authority, or bypassing the current V0.4.7 gate?

## Findings

### PASS — problem is real and product-relevant

The research demonstrates a repository shape where recursive whole-root semantic scanning is structurally wasteful and accuracy-risky: many deployables, multiple frontend generations, runtime-loaded plugins, mixed vendor/custom JavaScript, bundle indirection, tests, generated artifacts and non-.NET production code.

The separate operator observation of roughly 9 GB RAM on an unbounded run is consistent with the research risk profile, but the plan correctly does not turn that one hardware observation into a universal numeric gate.

### PASS — plan remains deterministic

The plan uses manifests, project/workspace metadata, bounded composition/runtime evidence and conservative classification. Claude/LLM reconnaissance remains research-only and is not a runtime dependency.

### PASS — classification separates role from scan mode

This avoids the false binary `vendor = ignore` model and allows a modified vendor tree to be indexed as runtime dependency while a narrow custom plugin is scanned as first-party.

### PASS — whole-repository coverage is preserved

The earlier informal idea of scanning only a recommended first slice was unsafe if presented as a full pack. The revised plan requires a complete repository scan plan and explicit READY/PARTIAL/FAILED coverage semantics, while allowing bounded execution waves.

### PASS — runtime/plugin edges preserve provenance

The research used the phrase `synthetic edges`; the plan corrects this. Runtime plugin edges require deterministic loader/build/copy identity evidence and carry provenance/confidence. Unsupported identity remains UNKNOWN.

### PASS — legacy JS does not require manual curation

The research suggested a file-level allowlist. The plan treats the effective first-party scan set as generated output from deterministic evidence rather than a user-maintained prerequisite.

### PASS — false cross-application links are explicitly guarded

Multi-owner shared modules are represented as shared nodes. Similar names never create edges. Tests are kept separate from production authority.

### PASS — discovery remains shallow

Discovery is prevented from becoming a second expensive semantic analysis pass. It uses tiered evidence and may inspect only bounded entry/composition files to resolve topology.

### PASS — memory plan attacks scope before Roslyn optimization

The plan first prevents vendor/generated/test-only areas from reaching deep scanners and introduces application-scoped execution waves. Shared analysis-context optimization remains aligned with the existing future W6 plan.

### PASS — private-repository privacy boundary is preserved

The committed research is already sanitized. Real-project validation is required to record only aliases/counts/classifications/resource deltas and must not copy source bodies, internal names, endpoints or config values into PKC reports.

### PASS — roadmap discipline is preserved

Current main still records V0.4.7-D/R7.10 pending independent rereview #17 and E/W locked. The plan is explicitly PREPARED / NOT YET UNLOCKED and cannot be used to bypass that gate.

## Rejected alternatives

### Reject: optimize C# semantic passes immediately

Reason: profiling evidence shows a large portion of the tree should never enter deep semantic scanning. Scope reduction is the first-order fix; semantic-context optimization comes afterward with regression protection.

### Reject: run Claude/LLM first as a production prerequisite

Reason: nondeterministic, external-cost dependent, privacy-sensitive and unnecessary for structural discovery that manifests/wiring can prove.

### Reject: directory-name exclusion rules

Reason: the real repository mixes first-party and vendor code under common legacy folder names and contains custom code inside a vendor tree.

### Reject: only follow project references

Reason: reflection-loaded plugins and build-copy behavior create supported runtime edges outside the project-reference graph.

### Reject: scan only the newest stack

Reason: useful for a development wave, but not a complete repository model. Legacy applications remain production-relevant until evidence proves otherwise.

### Reject: treat tracked bundles/minified output as ordinary source

Reason: they are runtime truth/evidence but may duplicate source and vendor code. Prefer bundle provenance mapping to source inputs.

### Reject: fixed absolute RAM threshold now

Reason: one observed machine/run is not enough to define portable acceptance. Structural boundedness plus controlled before/after measurements is stronger.

## Remaining design questions for the implementation session

These are implementation details to resolve with regressions, not blockers to the plan:

1. Exact internal schema/file split under `.pkc/discovery`.
2. Whether first implementation scopes scanners by root sets, file sets, application contexts, or a combination.
3. How much of solution/project evaluation can be shared with existing C# semantic loading without creating a second MSBuildWorkspace.
4. How to represent partial support for non-.NET production areas before a dedicated adapter exists.
5. Which coverage summary fields belong in the portable workspace versus local-only discovery metadata.
6. How to measure process peak memory portably in CI/private benchmark environments.

## Final decision

The plan is coherent, generic and evidence-driven enough to preserve as the execution packet for the future Repository Discovery / W5 refinement.

It is NOT an authorization to start that milestone today. The current repository gate remains authoritative.
