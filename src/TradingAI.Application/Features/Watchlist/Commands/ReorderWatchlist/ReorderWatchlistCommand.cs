using MediatR;

namespace TradingAI.Application.Features.Watchlist.Commands.ReorderWatchlist;

public record ReorderWatchlistCommand(Guid UserId, IReadOnlyList<Guid> AssetIdsInOrder) : IRequest;
