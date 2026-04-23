using MediatR;
using TradingAI.Application.Common.Models;

namespace TradingAI.Application.Assets.Queries.GetHistory;

public record GetHistoryQuery(Guid AssetId, string Interval, int Limit)
    : IRequest<IReadOnlyList<OhlcCandle>>;
