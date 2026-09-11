# PKC Status

Last updated: 2026-09-11

## Current milestone

**V0.3 frontend static evidence — COMPLETE**

Verified implementation commit: `26a7c381362dd3cf155974fc55883cbc72f8891a`
Verified golden commit: `1ee1d7bc2f7e5aff8f5d808a5d14b891144abee6`
GitHub Actions run: `34583992309` — SUCCESS

Verified:

- solution build
- C# evidence tests
- frontend static evidence tests
- end-to-end `pkc build samples/WorkPlaySample`
- UI facts in `.pkc/facts.json`
- feature candidate coverage includes `frontend-static`
- UI action/API route is linked to backend endpoint by HTTP method + normalized route
- generated Markdown contains UI route, action, permission guard and UI-to-backend call
- frontend unknown is removed when matching UI evidence exists
- golden Markdown exactly matches regenerated output

## Current commands

```bash
pkc scan <repository-path>
pkc build <repository-path>
```

Outputs:

```text
.pkc/facts.json
.pkc/feature-candidates.json
knowledge/features/**/*.md
```

## AI-testable WorkPlay knowledge

```text
samples/WorkPlaySample/knowledge/features/workplay/complete.md
```

The sample now grounds these answers:

- open `/workplays/:id`
- click `Complete` on `WorkPlayDetailPage`
- UI guard: `ManageWorkPlay`
- UI sends the Complete request to the WorkPlay backend endpoint
- backend requires `ManageWorkPlay`
- Completed WorkPlay cannot change status
- status is changed and a status-changed publication-like call occurs

## Countdown to AI test

**0 steps remaining.**

The Markdown contains backend + frontend-static knowledge and can be attached directly to another AI.

## Current limitation

V0.3 intentionally uses a conservative React/TypeScript static scanner for common patterns. It is not yet a full TypeScript AST engine and does not claim runtime-confirmed UI behavior.

## Next target

**V0.4 — Azure DevOps evidence**

Add Epic/Feature/PBI, Sprint/iteration, work-item state, acceptance criteria/description, selected history/revisions, PR/commit traceability, and link delivery evidence to generated product features.
