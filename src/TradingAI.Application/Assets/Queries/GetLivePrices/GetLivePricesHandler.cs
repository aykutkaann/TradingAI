using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Assets.DTOs;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Assets.Queries.GetLivePrices;

public class GetLivePricesHandler(IApplicationDbContext db, IMarketDataService marketData)
    : IRequestHandler<GetLivePricesQuery, IReadOnlyList<PriceDto>>
{
    public async Task<IReadOnlyList<PriceDto>> Handle(GetLivePricesQuery request, CancellationToken ct)
    {
        var assets = await db.Assets.AsNoTracking()
            .Where(a => request.AssetIds.Contains(a.Id) && a.IsActive)
            .ToListAsync(ct);

        if (assets.Count == 0) return Array.Empty<PriceDto>();

        var priceResults = await marketData.GetBatchPriceAsync(assets, ct);

        var result = new List<PriceDto>();
        foreach (var asset in assets)
        {
            // PriceData.Symbol stores the pair (see MarketDataService mapping)
            var p = priceResults.FirstOrDefault(x => x.Symbol == asset.Pair);
            if (p is null) continue;

            result.Add(new PriceDto(
                AssetId: asset.Id,
                Pair: asset.Pair,
                CurrentPrice: p.CurrentPrice,
                Change24h: p.Change24h,
                ChangePercent24h: p.ChangePercenth24h,
                High24h: p.High24h,
                Low24h: p.Low24h,
                Volume24h: p.Volume24h,
                UpdatedAt: p.UpdatedAt
            ));
        }

        return result;
    }
}
