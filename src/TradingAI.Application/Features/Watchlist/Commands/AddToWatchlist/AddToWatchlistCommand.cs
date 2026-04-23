using MediatR;

namespace TradingAI.Application.Features.Watchlist.Commands.AddToWatchlist;

public record AddToWatchlistCommand(Guid UserId, Guid AssetId) : IRequest<Guid>;
