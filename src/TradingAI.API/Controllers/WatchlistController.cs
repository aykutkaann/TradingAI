using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingAI.API.Extensions;
using TradingAI.API.Watchlist.DTOs;
using TradingAI.Application.Watchlist.Commands.AddToWatchlist;
using TradingAI.Application.Watchlist.Commands.RemoveFromWatchlist;
using TradingAI.Application.Watchlist.Commands.ReorderWatchlist;
using TradingAI.Application.Watchlist.DTOs;
using TradingAI.Application.Watchlist.Queries.GetWatchlist;

namespace TradingAI.API.Controllers;

[ApiController]
[Route("api/watchlist")]
[Authorize]
public class WatchlistController(IMediator mediator) : ControllerBase
{
    // GET /api/watchlist
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WatchlistItemDto>>> Get()
    {
        var userId = User.GetUserId();
        var result = await mediator.Send(new GetWatchlistQuery(userId));
        return Ok(result);
    }

    // POST /api/watchlist
    [HttpPost]
    public async Task<IActionResult> Add(AddToWatchlistDto dto)
    {
        var userId = User.GetUserId();
        var id = await mediator.Send(new AddToWatchlistCommand(userId, dto.AssetId));
        return Ok(new { id });
    }

    // DELETE /api/watchlist/{assetId}
    [HttpDelete("{assetId:guid}")]
    public async Task<IActionResult> Remove(Guid assetId)
    {
        var userId = User.GetUserId();
        await mediator.Send(new RemoveFromWatchlistCommand(userId, assetId));
        return NoContent();
    }

    // PUT /api/watchlist/reorder
    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder(ReorderWatchlistDto dto)
    {
        var userId = User.GetUserId();
        await mediator.Send(new ReorderWatchlistCommand(userId, dto.AssetIds));
        return NoContent();
    }
}
