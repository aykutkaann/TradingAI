using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Features.Watchlist.Commands.ReorderWatchlist;

public class ReorderWatchlistHandler(IApplicationDbContext db)
    : IRequestHandler<ReorderWatchlistCommand>
{
    public async Task Handle(ReorderWatchlistCommand request, CancellationToken ct)
    {
        var items = await db.WatchLists
            .Where(w => w.UserId == request.UserId)
            .ToListAsync(ct);

        for (int i = 0; i < request.AssetIdsInOrder.Count; i++)
        {
            var assetId = request.AssetIdsInOrder[i];
            var item = items.FirstOrDefault(w => w.AssetId == assetId);
            if (item is not null)
                item.SortOrder = i;
        }

        await db.SaveChangesAsync(ct);
    }
}
