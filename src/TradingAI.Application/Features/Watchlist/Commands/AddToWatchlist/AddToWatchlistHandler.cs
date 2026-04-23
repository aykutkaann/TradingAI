using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Domain.Entities;

namespace TradingAI.Application.Features.Watchlist.Commands.AddToWatchlist;

public class AddToWatchlistHandler(IApplicationDbContext db)
    : IRequestHandler<AddToWatchlistCommand, Guid>
{
    public async Task<Guid> Handle(AddToWatchlistCommand request, CancellationToken ct)
    {
        var assetExists = await db.Assets
            .AnyAsync(a => a.Id == request.AssetId && a.IsActive, ct);

        if (!assetExists)
            throw new NotFoundException("Asset not found");

        var alreadyInList = await db.WatchLists
            .AnyAsync(w => w.UserId == request.UserId && w.AssetId == request.AssetId, ct);

        if (alreadyInList)
            throw new ConflictException("Asset already in watchlist");

        var maxOrder = await db.WatchLists
            .Where(w => w.UserId == request.UserId)
            .Select(w => (int?)w.SortOrder)
            .MaxAsync(ct) ?? -1;

        var item = new WatchListItem
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            AssetId = request.AssetId,
            SortOrder = maxOrder + 1,
            AddedAt = DateTime.UtcNow
        };

        db.WatchLists.Add(item);
        await db.SaveChangesAsync(ct);

        return item.Id;
    }
}
