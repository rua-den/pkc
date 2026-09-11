using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WorkPlaySample;

public enum WorkPlayStatus
{
    Draft,
    Active,
    Completed,
    Cancelled
}

public sealed class WorkPlay
{
    public int Id { get; init; }
    public WorkPlayStatus Status { get; private set; } = WorkPlayStatus.Draft;

    public void SetStatus(WorkPlayStatus status) => Status = status;
}

public sealed class WorkPlayService
{
    public void ChangeStatus(WorkPlay workPlay, WorkPlayStatus targetStatus)
    {
        if (workPlay.Status == WorkPlayStatus.Completed)
        {
            throw new InvalidOperationException("Completed WorkPlay cannot change status.");
        }

        workPlay.SetStatus(targetStatus);
        PublishStatusChanged(workPlay);
    }

    private static void PublishStatusChanged(WorkPlay workPlay)
    {
        _ = workPlay.Id;
    }
}

[ApiController]
[Route("api/workplays")]
public sealed class WorkPlayController : ControllerBase
{
    private readonly WorkPlayService _service = new();

    [HttpPost("{id:int}/complete")]
    [Authorize(Policy = "ManageWorkPlay")]
    public IActionResult Complete(int id)
    {
        var workPlay = new WorkPlay { Id = id };
        _service.ChangeStatus(workPlay, WorkPlayStatus.Completed);
        return Ok();
    }
}
