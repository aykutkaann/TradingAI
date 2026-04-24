using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Enums;

namespace TradingAI.Application.Features.Subscriptions.DTOs
{

    public record SubscriptionDto(SubscriptionTier Tier, string PlanName, int DailyAnalysisLimit, int DailyPublishLimit,
        bool CanUseAssetAnalysis, bool CanAccessLeaderBoard, int MaxImageSizeMb, DateTime? ExpiresAt, bool IsActive, DateTime StartedAt);
}
