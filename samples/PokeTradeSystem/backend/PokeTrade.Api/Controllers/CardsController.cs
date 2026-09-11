using Microsoft.AspNetCore.Mvc;

namespace PokeTrade.Api.Controllers;

[ApiController]
[Route("api/cards")]
public sealed class CardsController(PokeTradeStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<PokemonCard>> GetCards() => Ok(store.GetCards());
}
