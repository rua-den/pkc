using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PokeTrade.Api.Controllers;

[ApiController]
[Route("api/deliveries")]
public sealed class DeliveriesController(PokeTradeStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Delivery>> GetDeliveries() => Ok(store.GetDeliveries());

    [HttpPatch("{id:int}/dispatch")]
    [Authorize(Policy = "ManageDelivery")]
    public ActionResult<Delivery> DispatchDelivery(int id)
    {
        try { return Ok(store.DispatchDelivery(id)); }
        catch (InvalidOperationException exception) { return Conflict(new { message = exception.Message }); }
        catch (KeyNotFoundException exception) { return NotFound(new { message = exception.Message }); }
    }

    [HttpPatch("{id:int}/delivered")]
    [Authorize(Policy = "ManageDelivery")]
    public ActionResult<Delivery> MarkDeliveryDelivered(int id)
    {
        try { return Ok(store.MarkDeliveryDelivered(id)); }
        catch (InvalidOperationException exception) { return Conflict(new { message = exception.Message }); }
        catch (KeyNotFoundException exception) { return NotFound(new { message = exception.Message }); }
    }
}
