using MediatR;
using TradingAI.Application.Watchlist.DTOs;

namespace TradingAI.Application.Watchlist.Queries.GetWatchlist;

public record GetWatchlistQuery(Guid UserId) : IRequest<IReadOnlyList<WatchlistItemDto>>;
