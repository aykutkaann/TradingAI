using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Features.Watchlist.Commands.RemoveFromWatchlist;

public class RemoveFromWatchlistHandler(IApplicationDbContext db)
    : IRequestHandler<RemoveFromWatchlistCommand>
{
    public async Task Handle(RemoveFromWatchlistCommand request, CancellationToken ct)
    {
        var item = await db.WatchLists
            .FirstOrDefaultAsync(w => w.UserId == request.UserId && w.AssetId == request.AssetId, ct);

        if (item is null)
            throw new NotFoundException("Watchlist item not found");

        db.WatchLists.Remove(item);
        await db.SaveChangesAsync(ct);
    }
}
