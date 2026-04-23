using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Assets.DTOs;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Features.Watchlist.DTOs;

namespace TradingAI.Application.Features.Watchlist.Queries.GetWatchlist;

public class GetWatchlistHandler(IApplicationDbContext db, IMarketDataService marketData)
    : IRequestHandler<GetWatchlistQuery, IReadOnlyList<WatchlistItemDto>>
{
    public async Task<IReadOnlyList<WatchlistItemDto>> Handle(GetWatchlistQuery request, CancellationToken ct)
    {
        var items = await db.WatchLists
            .AsNoTracking()
            .Include(w => w.Asset)
            .Where(w => w.UserId == request.UserId)
            .OrderBy(w => w.SortOrder)
            .ToListAsync(ct);

        if (items.Count == 0) return Array.Empty<WatchlistItemDto>();

        var assets = items.Select(i => i.Asset).ToList();
        var prices = await marketData.GetBatchPriceAsync(assets, ct);

        var result = new List<WatchlistItemDto>();
        foreach (var item in items)
        {
            var p = prices.FirstOrDefault(x => x.Symbol == item.Asset.Pair);

            PriceDto? priceDto = p is null ? null : new PriceDto(
                AssetId: item.AssetId,
                Pair: item.Asset.Pair,
                CurrentPrice: p.CurrentPrice,
                Change24h: p.Change24h,
                ChangePercent24h: p.ChangePercenth24h,
                High24h: p.High24h,
                Low24h: p.Low24h,
                Volume24h: p.Volume24h,
                UpdatedAt: p.UpdatedAt
            );

            result.Add(new WatchlistItemDto(
                Id: item.Id,
                AssetId: item.AssetId,
                Symbol: item.Asset.Symbol,
                Name: item.Asset.Name,
                Pair: item.Asset.Pair,
                Type: item.Asset.Type,
                SortOrder: item.SortOrder,
                Price: priceDto
            ));
        }

        return result;
    }
}
