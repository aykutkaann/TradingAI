using MediatR;

using TradingAI.Application.Assets.DTOs;
using TradingAI.Application.Common.Models;

namespace TradingAI.Application.Assets.Queries.GetLivePrices
{

    public record GetLivePricesQuery(IReadOnlyList<Guid> AssetIds) : IRequest<IReadOnlyList<PriceDto>>;
}
