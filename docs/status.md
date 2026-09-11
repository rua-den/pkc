# PKC Status

Last updated: 2026-09-11

## Current milestone

**V0.3 frontend static evidence — IN PROGRESS**

Implementation commit prepared: `c57537f9287cfeee8f2bd100e40fb637a6a87d94`

This slice adds React/TypeScript static UI evidence and connects matching UI API calls to backend endpoint feature candidates using HTTP method + normalized route.

## Target evidence in V0.3

- React screen/component facts
- React Router route facts
- button/action facts
- common permission guards such as `hasPermission(...)`, `can(...)`, `canAccess(...)`
- `fetch(...)` and common client `.post/.put/.patch/.delete/.get` API calls
- UI action -> API -> backend feature linkage
- Markdown sections describing how to perform the action in the UI

## Current command target

```bash
pkc build <repository-path>
```

Expected outputs remain:

```text
.pkc/facts.json
.pkc/feature-candidates.json
knowledge/features/**/*.md
```

## Countdown to Markdown with UI instructions

**1 verification step remaining:** CI must prove build/tests/E2E and confirm the WorkPlay sample Markdown contains the route, Complete action, UI permission guard and matched backend API without retaining the old frontend-unknown warning.

After that, the next major input source is Azure DevOps delivery/product history.
