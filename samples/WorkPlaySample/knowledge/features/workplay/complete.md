---
id: "feature:workplay:complete"
title: "WorkPlay Complete"
area: "WorkPlay"
authority: "code-observed"
generated: true
coverage:
  - backend-code
  - frontend-static
---

# WorkPlay Complete

> This file describes behavior observed in the current implementation. It is not yet business-approved truth.

## What this file represents

Observed UI and backend behavior for the Complete action in WorkPlay.

## How to do it in the UI

- Open route `/workplays/:id`.
- Click `Complete` on `WorkPlayDetailPage`; UI guard: `ManageWorkPlay`.

## UI to backend

- UI sends `POST /api/workplays/${id}/complete`.

## Backend entry point

- POST /api/workplays/{id:int}/complete

## Permissions

- Policy: ManageWorkPlay
- UI visibility guard: ManageWorkPlay

## Observed business rules

- When `workPlay.Status == WorkPlayStatus.Completed`, the implementation throws `InvalidOperationException` using `new InvalidOperationException("Completed WorkPlay cannot change status.")`.

## State changes

- Sets `Status` to `status`.

## Side effects

- Calls publication-like method `WorkPlaySample.WorkPlayService.PublishStatusChanged`.

## Backend flow

- WorkPlaySample.WorkPlayService.ChangeStatus → WorkPlaySample.WorkPlay.SetStatus
- WorkPlaySample.WorkPlayService.ChangeStatus → WorkPlaySample.WorkPlayService.PublishStatusChanged
- WorkPlaySample.WorkPlayController.Complete → WorkPlaySample.WorkPlayService.ChangeStatus

## Important unknowns

- Azure DevOps delivery history and product intent have not been analyzed yet.

## Evidence

- `WorkPlayDetail.tsx:L1-L1` — UI screen/component WorkPlayDetailPage
- `WorkPlayDetail.tsx:L3-L3` — UI API call POST /api/workplays/${id}/complete
- `WorkPlayDetail.tsx:L7-L7` — UI action Complete
- `WorkPlayDetail.tsx:L11-L11` — UI screen/component Routes
- `WorkPlayDetail.tsx:L12-L12` — UI route /workplays/:id
- `WorkPlayFeature.cs:L19-L19` — Method WorkPlaySample.WorkPlay.SetStatus
- `WorkPlayFeature.cs:L19-L19` — Mutation: Status
- `WorkPlayFeature.cs:L24-L33` — Method WorkPlaySample.WorkPlayService.ChangeStatus
- `WorkPlayFeature.cs:L26-L29` — Condition: workPlay.Status == WorkPlayStatus.Completed
- `WorkPlayFeature.cs:L28-L28` — Throws InvalidOperationException
- `WorkPlayFeature.cs:L35-L38` — Method WorkPlaySample.WorkPlayService.PublishStatusChanged
- `WorkPlayFeature.cs:L47-L54` — Endpoint WorkPlaySample.WorkPlayController.Complete
