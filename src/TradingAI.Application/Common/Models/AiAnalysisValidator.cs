using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Common.Models
{

    public static class AiAnalysisValidator
    {
        /// <summary>
        /// Returns a corrected AiAnalysisResult. If SL/TP levels contradict the trend direction,
        /// nulls them out rather than saving nonsense to the DB.
        /// </summary>
        public static AiAnalysisResult SanitizeTradeLevels(AiAnalysisResult r, ILogger? logger = null)
        {
            // Only check if we have entry + trend
            if (r.SuggestedEntry is null) return r;

            var entry = r.SuggestedEntry.Value;
            var trend = r.TrendDirection?.Trim().ToLowerInvariant();

            bool valid = trend switch
            {
                "bullish" => IsValidLong(entry, r.StopLoss, r.TakeProfit1, r.TakeProfit2),
                "bearish" => IsValidShort(entry, r.StopLoss, r.TakeProfit1, r.TakeProfit2),
                _ => true  // neutral / unknown: skip check
            };

            if (valid) return r;

            logger?.LogWarning(
                "AI returned inconsistent trade levels. Trend={Trend} Entry={Entry} SL={SL} TP1={TP1} TP2={TP2}. Nulling levels.",
                trend, entry, r.StopLoss, r.TakeProfit1, r.TakeProfit2);

            // Wipe the bad levels but keep the narrative analysis
            return r with
            {
                SuggestedEntry = null,
                StopLoss = null,
                TakeProfit1 = null,
                TakeProfit2 = null,
                RiskRewardRatio = null
            };
        }

        private static bool IsValidLong(decimal entry, decimal? sl, decimal? tp1, decimal? tp2)
        {
            if (sl is not null && sl >= entry) return false;
            if (tp1 is not null && tp1 <= entry) return false;
            if (tp2 is not null && tp1 is not null && tp2 <= tp1) return false;
            return true;
        }

        private static bool IsValidShort(decimal entry, decimal? sl, decimal? tp1, decimal? tp2)
        {
            if (sl is not null && sl <= entry) return false;
            if (tp1 is not null && tp1 >= entry) return false;
            if (tp2 is not null && tp1 is not null && tp2 >= tp1) return false;
            return true;
        }
    }
}
