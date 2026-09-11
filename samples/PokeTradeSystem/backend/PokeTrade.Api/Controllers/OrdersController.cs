using Microsoft.AspNetCore.Mvc;

namespace PokeTrade.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(PokeTradeStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Order>> GetOrders() => Ok(store.GetOrders());

    [HttpPost]
    public ActionResult<Order> CreateOrder(CreateOrderRequest request)
    {
        try
        {
            var order = store.CreateOrder(request);
            return Created($"/api/orders/{order.Id}", order);
        }
        catch (Exception exception) when (exception is InvalidOperationException or KeyNotFoundException)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
