using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Common.Models
{

    public record OhlcCandle(DateTime Timestamp, decimal Open, decimal High, decimal Low, decimal Close, decimal Volume);
}
