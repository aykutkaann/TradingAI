using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Assets.DTOs
{

    public record PriceDto(Guid AssetId, string Pair, decimal CurrentPrice,
        decimal Change24h, decimal ChangePercent24h, decimal High24h,
        decimal Low24h, decimal Volume24h, DateTime UpdatedAt);
}
