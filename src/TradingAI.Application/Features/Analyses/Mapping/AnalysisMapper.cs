using System.Text.Json;
using TradingAI.Application.Features.Analyses.Dtos;
using TradingAI.Domain.Entities;

namespace TradingAI.Application.Features.Analyses.Mapping;

public static class AnalysisMapper
{
    private sealed record KeyLevelsJson(List<decimal>? Support, List<decimal>? Resistance);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };


    public static AnalysisDto ToDto(Analysis a)
    {
        // Defensive: if navigation wasn't included, fail loudly instead of silently returning null.
        if (a.User is null)
            throw new InvalidOperationException(
                $"Analysis {a.Id}: User navigation not loaded. Did you forget .Include(a => a.User)?");

        var patterns = string.IsNullOrWhiteSpace(a.DetectedPatterns)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(a.DetectedPatterns) ?? new List<string>();

        var keyLevels = string.IsNullOrWhiteSpace(a.KeyLevels)
            ? new KeyLevelsJson(null, null)
            : JsonSerializer.Deserialize<KeyLevelsJson>(a.KeyLevels, JsonOpts)
              ?? new KeyLevelsJson(null, null);

        return new AnalysisDto(
            Id: a.Id,
            UserId: a.User.Id,
            UserDisplayName: a.User.DisplayName ?? a.User.UserName,
            AssetId: a.Asset?.Id,
            AssetSymbol: a.Asset?.Symbol,
            AssetPair: a.Pair,
            TimeFrame: a.TimeFrame,
            ChartImageUrl: a.ImageUrl,
            TrendDirection: a.TrendDirection ?? "Neutral",
            DetectedPaterns: patterns,
            SupportLevels: keyLevels.Support ?? new List<decimal>(),
            ResistanceLevels: keyLevels.Resistance ?? new List<decimal>(),
            SuggestedEntry: a.SuggestedEntry,
            StopLoss: a.StopLoss,
            TakeProfit1: a.TakeProfit1,
            TakeProfit2: a.TakeProfit2,
            RiskRewardRatio: a.RiskRewardRatio,
            Analysis: a.AiAnalysis,
            Summary: a.Summary ?? string.Empty,
            IsPublished: a.IsPublished,
            LikeCount: a.Likes?.Count ?? 0,
            CommentCount: a.Comments?.Count ?? 0,
            CreatedAt: a.CreatedAt
        );
    }
}