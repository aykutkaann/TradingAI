using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.Models;

namespace TradingAI.Application.Assets.Queries.GetHistory;

public class GetHistoryHandler(IApplicationDbContext db, IMarketDataService marketData)
    : IRequestHandler<GetHistoryQuery, IReadOnlyList<OhlcCandle>>
{
    public async Task<IReadOnlyList<OhlcCandle>> Handle(GetHistoryQuery request, CancellationToken ct)
    {
        var asset = await db.Assets.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AssetId && a.IsActive, ct);

        if (asset is null)
            throw new NotFoundException("Asset not found");

        return await marketData.GetHistoricalDataAsync(asset, request.Interval, request.Limit, ct);
    }
}
