using MediatR;

namespace TradingAI.Application.Watchlist.Commands.RemoveFromWatchlist;

public record RemoveFromWatchlistCommand(Guid UserId, Guid AssetId) : IRequest;
