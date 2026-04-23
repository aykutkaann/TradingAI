using MediatR;

namespace TradingAI.Application.Watchlist.Commands.ReorderWatchlist;

public record ReorderWatchlistCommand(Guid UserId, IReadOnlyList<Guid> AssetIdsInOrder) : IRequest;
