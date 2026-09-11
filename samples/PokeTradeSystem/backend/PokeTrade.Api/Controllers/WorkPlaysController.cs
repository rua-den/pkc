using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PokeTrade.Api.Controllers;

[ApiController]
[Route("api/workplays")]
public sealed class WorkPlaysController(PokeTradeStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<WorkPlay>> GetWorkPlays() => Ok(store.GetWorkPlays());

    [HttpPatch("{id:int}/start")]
    [Authorize(Policy = "ManageWorkPlay")]
    public ActionResult<WorkPlay> StartWorkPlay(int id)
    {
        try { return Ok(store.StartWorkPlay(id)); }
        catch (InvalidOperationException exception) { return Conflict(new { message = exception.Message }); }
        catch (KeyNotFoundException exception) { return NotFound(new { message = exception.Message }); }
    }

    [HttpPatch("{id:int}/complete")]
    [Authorize(Policy = "ManageWorkPlay")]
    public ActionResult<WorkPlay> CompleteWorkPlay(int id, CompleteWorkPlayRequest request)
    {
        try { return Ok(store.CompleteWorkPlay(id, request)); }
        catch (InvalidOperationException exception) { return Conflict(new { message = exception.Message }); }
        catch (KeyNotFoundException exception) { return NotFound(new { message = exception.Message }); }
    }
}
