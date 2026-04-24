using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Enums;

namespace TradingAI.Application.Features.Subscriptions.Models;

public record Entitlements(
    SubscriptionTier Tier,
    int DailyAnalysisLimit,
    int DailyPublishLimit,
    bool CanUseAssetAnalysis,
    bool CanAccessLeaderboard,
    bool CanSeePlatformStats,
    int MaxImageSizeMb,
    DateTime? ExpiresAt
);


