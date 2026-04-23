using MediatR;

namespace TradingAI.Application.Watchlist.Commands.AddToWatchlist;

public record AddToWatchlistCommand(Guid UserId, Guid AssetId) : IRequest<Guid>;
