using MediatR;
using TradingAI.Application.Features.Watchlist.DTOs;

namespace TradingAI.Application.Features.Watchlist.Queries.GetWatchlist;

public record GetWatchlistQuery(Guid UserId) : IRequest<IReadOnlyList<WatchlistItemDto>>;
