using MediatR;

namespace TradingAI.Application.Features.Watchlist.Commands.RemoveFromWatchlist;

public record RemoveFromWatchlistCommand(Guid UserId, Guid AssetId) : IRequest;
