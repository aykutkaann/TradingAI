using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.Analyses.Dtos
{

    public record AnalysisDto(
        Guid Id, Guid UserId, string UserDisplayName, Guid? AssetId, string? AssetSymbol, string? AssetPair, string TimeFrame,
        string? ChartImageUrl, string TrendDirection, IReadOnlyList<string> DetectedPaterns, IReadOnlyList<decimal> SupportLevels,
        IReadOnlyList<decimal> ResistanceLevels, decimal? SuggestedEntry, decimal? StopLoss, decimal? TakeProfit1, decimal? TakeProfit2,
        decimal? RiskRewardRatio, string Analysis, string Summary, bool IsPublished, int LikeCount, int CommentCount, DateTime CreatedAt
        ,bool IsLikedByMe= false);
}
