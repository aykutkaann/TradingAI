using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Common.Models
{
    public record PriceData(string Symbol, decimal CurrentPrice, decimal Change24h, decimal ChangePercenth24h,
        decimal High24h, decimal Low24h, decimal Volume24h, DateTime UpdatedAt);
    
    
}
