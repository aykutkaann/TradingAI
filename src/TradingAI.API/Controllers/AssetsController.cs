using MediatR;
using Microsoft.AspNetCore.Mvc;
using TradingAI.Application.Assets.DTOs;
using TradingAI.Application.Assets.Queries.GetAssets;
using TradingAI.Application.Assets.Queries.GetHistory;
using TradingAI.Application.Assets.Queries.GetLivePrices;
using TradingAI.Application.Common.Models;
using TradingAI.Domain.Enums;

namespace TradingAI.API.Controllers;

[ApiController]
[Route("api/assets")]
public class AssetsController(IMediator mediator) : ControllerBase
{
    // GET /api/assets?type=Crypto
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssetDto>>> List([FromQuery] AssetType? type)
    {
        var result = await mediator.Send(new GetAssetsQuery(type));
        return Ok(result);
    }

    // GET /api/assets/prices?ids=guid1,guid2,guid3
    [HttpGet("prices")]
    public async Task<ActionResult<IReadOnlyList<PriceDto>>> Prices([FromQuery] string ids)
    {
        if (string.IsNullOrWhiteSpace(ids))
            return Ok(Array.Empty<PriceDto>());

        var parsed = ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(s => Guid.TryParse(s, out var g) ? g : (Guid?)null)
                        .Where(g => g.HasValue)
                        .Select(g => g!.Value)
                        .ToList();

        if (parsed.Count == 0)
            return BadRequest(new { message = "No valid asset ids provided." });

        var result = await mediator.Send(new GetLivePricesQuery(parsed));
        return Ok(result);
    }

    // GET /api/assets/{id}/history?interval=1D&limit=100
    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<OhlcCandle>>> History(
        Guid id,
        [FromQuery] string interval = "1D",
        [FromQuery] int limit = 100)
    {
        if (limit <= 0 || limit > 1000)
            return BadRequest(new { message = "limit must be between 1 and 1000." });

        var result = await mediator.Send(new GetHistoryQuery(id, interval, limit));
        return Ok(result);
    }
}
