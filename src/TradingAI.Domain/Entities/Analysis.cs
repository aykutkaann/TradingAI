using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Enums;

namespace TradingAI.Domain.Entities
{
    public class Analysis
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? AssetId { get; set; }


        //Inputs
        public AnalysisType Type  { get; set; }
        public string? ImageUrl { get; set; }
        public string? UserPrompt { get; set; }
        public string TimeFrame { get; set; }  //"5m" "1h", "4h", "1d", "1w"

        public string Pair { get; set; }

        //AI outputs

        public string AiAnalysis { get; set; }
        public string? TrendDirection { get; set; } //Bearish, bullish, neutral
        public decimal? SuggestedEntry { get; set; }
        public decimal? StopLoss { get; set; }
        public decimal? TakeProfit1 { get; set; }
        public decimal? TakeProfit2 { get; set; }
        public decimal? RiskRewardRatio { get; set; }
        public string? DetectedPatterns { get; set; }
        public string? KeyLevels { get; set; }
        public string? Summary { get; set; }

        //Tracking  
        public decimal? PriceAtAnalysis { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }


        public bool TakeProfit1Hit { get; set; }
        public bool TakeProfit2Hit { get; set; }
        public bool StopLossHit { get; set; }
        public AnalysisOutcome Outcome { get; set; } = AnalysisOutcome.Pending;   // Hit TP, Hit SL, Neutral
        public DateTime? OutcomeCheckedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public decimal? ResolvedPrice { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // Navigation
        public User User { get; set; }
        public Asset? Asset { get; set; }
        public ICollection<AnalysisComment> Comments { get; set; } = new HashSet<AnalysisComment>();
        public ICollection<AnalysisLike> Likes { get; set; } = new HashSet<AnalysisLike>();

    }
}
