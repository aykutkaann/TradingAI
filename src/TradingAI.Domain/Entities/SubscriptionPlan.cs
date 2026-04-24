using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Enums;

namespace TradingAI.Domain.Entities
{
    public class SubscriptionPlan
    {
        public Guid Id { get; set; }
        public SubscriptionTier Tier { get; set; }
        public string Name { get; set; }
        public decimal PriceWeeklyUsd { get; set; }
        public decimal PriceMonthlyUsd { get; set; }
        public decimal PriceYearlyUsd { get; set; }

        //Entitlements

        public int DailyAnalysisLimit { get; set; }
        public int DailyPublishLimit { get; set; }
        public bool CanUseAssetAnalysis { get; set; }
        public bool CanAccessLeaderBoard { get; set; }
        public bool CanSeePlatformStats { get; set; }
        public int MaxImageSizeMb { get; set; }


        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

    }
}
