---
id: "feature:workplay:complete"
title: "WorkPlay Complete"
area: "WorkPlay"
authority: "code-observed"
generated: true
coverage:
  - backend-code
---

# WorkPlay Complete

> This file describes behavior observed in the current implementation. It is not yet business-approved truth.

## What this file represents

Code-observed backend behavior for the Complete action in WorkPlay.

## Backend entry point

- POST /api/workplays/{id:int}/complete

## Permissions

- Policy: ManageWorkPlay

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

- Frontend/UI entry point and user interaction path have not been analyzed yet.
- Azure DevOps delivery history and product intent have not been analyzed yet.

## Evidence

- `WorkPlayFeature.cs:L19-L19` — Method WorkPlaySample.WorkPlay.SetStatus
- `WorkPlayFeature.cs:L19-L19` — Mutation: Status
- `WorkPlayFeature.cs:L24-L33` — Method WorkPlaySample.WorkPlayService.ChangeStatus
- `WorkPlayFeature.cs:L26-L29` — Condition: workPlay.Status == WorkPlayStatus.Completed
- `WorkPlayFeature.cs:L28-L28` — Throws InvalidOperationException
- `WorkPlayFeature.cs:L35-L38` — Method WorkPlaySample.WorkPlayService.PublishStatusChanged
- `WorkPlayFeature.cs:L47-L54` — Endpoint WorkPlaySample.WorkPlayController.Complete
