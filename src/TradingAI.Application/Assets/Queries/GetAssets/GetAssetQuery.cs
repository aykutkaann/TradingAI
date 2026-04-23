using MediatR;

using TradingAI.Application.Assets.DTOs;
using TradingAI.Domain.Enums;

namespace TradingAI.Application.Assets.Queries.GetAssets
{

    public record GetAssetsQuery(AssetType? Type) : IRequest<IReadOnlyList<AssetDto>>;
}
