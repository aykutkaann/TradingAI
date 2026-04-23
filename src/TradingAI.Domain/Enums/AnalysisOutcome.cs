using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Domain.Enums
{
    public enum  AnalysisOutcome
    {
        Pending = 0,
        TakeProfit1Hit = 1,
        TakeProfit2Hit = 2,
        StopLossHit = 3,
        Neutral = 4

    }
}
