using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Common.Models
{

    public record PriceCandle(DateTime Time, decimal Open, decimal High, decimal Low, decimal Close);
}
