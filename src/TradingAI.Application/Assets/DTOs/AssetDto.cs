using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Enums;

namespace TradingAI.Application.Assets.DTOs
{

    public record AssetDto(Guid Id, string Symbol, string Name, string Pair, AssetType Type);
}
