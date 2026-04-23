using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Common.Models
{


    public record AiAnalysisResult(
        string TrendDirection,            // "Bullish" | "Bearish" | "Neutral"
        IReadOnlyList<string> DetectedPatterns,
        KeyLevels KeyLevels,
        decimal? SuggestedEntry,
        decimal? StopLoss,
        decimal? TakeProfit1,
        decimal? TakeProfit2,
        decimal? RiskRewardRatio,
        string Analysis,                  // long markdown
        string Summary                    // 2-3 sentence
    );

    public record KeyLevels(
        IReadOnlyList<decimal> Support,
        IReadOnlyList<decimal> Resistance
    );
}
