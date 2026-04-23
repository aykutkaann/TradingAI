using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Features.Analyses.Dtos;
using TradingAI.Domain.Entities;

namespace TradingAI.Application.Features.Analyses.Queries.GetAnalysisById
{
    public class GetAnalysisByIdQueryHandler(IApplicationDbContext db) : IRequestHandler<GetAnalysisByIdQuery, AnalysisDto>
    {
        public async Task<AnalysisDto> Handle(GetAnalysisByIdQuery request, CancellationToken ct)
        {

            var analysis = await db.Analyses
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Asset)          // null for image uploads — that's fine, EF handles it
                .FirstOrDefaultAsync(a => a.Id == request.AnalysisId, ct);

            if (analysis is null)
                throw new NotFoundException("Analysis not found");

            var isOwner = request.CurrentUserId.HasValue && analysis.UserId == request.CurrentUserId.Value;
            if(!isOwner && !analysis.IsPublished)
                throw new NotFoundException("Analysis  not found.");


            return MapToDto(analysis, analysis.User, analysis.Asset);
        }

        private sealed record KeyLevelsJson(List<decimal>? Support, List<decimal>? Resistance);

        private AnalysisDto MapToDto(Analysis analysis, User user, Asset? asset = null)
        {

            var patterns = JsonSerializer.Deserialize<List<string>>(analysis.DetectedPatterns ?? "[]") ?? new List<string>();

            var keyLevelsJson = string.IsNullOrWhiteSpace(analysis.KeyLevels)
                        ? new KeyLevelsJson(null, null)
                        : JsonSerializer.Deserialize<KeyLevelsJson>(analysis.KeyLevels,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                          ?? new KeyLevelsJson(null, null);

            var supportLevels = keyLevelsJson.Support ?? new List<decimal>();
            var resistanceLevels = keyLevelsJson.Resistance ?? new List<decimal>();


            return new AnalysisDto(
                Id: analysis.Id,
                UserId: user.Id,
                UserDisplayName: user.DisplayName ?? user.UserName,
                AssetId: asset?.Id,
                AssetSymbol: asset?.Symbol,
                AssetPair: analysis.Pair,
                TimeFrame: analysis.TimeFrame,
                ChartImageUrl: analysis.ImageUrl,
                TrendDirection: analysis.TrendDirection,
                DetectedPaterns: patterns,
                SupportLevels: supportLevels,
                ResistanceLevels: resistanceLevels,
                SuggestedEntry: analysis.SuggestedEntry,
                StopLoss: analysis.StopLoss,
                TakeProfit1: analysis.TakeProfit1,
                TakeProfit2: analysis.TakeProfit2,
                RiskRewardRatio: analysis.RiskRewardRatio,
                Analysis: analysis.AiAnalysis,
                Summary: analysis.Summary,
                IsPublished: analysis.IsPublished,
                LikeCount: analysis.Likes?.Count ?? 0,
                CommentCount: analysis.Comments?.Count ?? 0,
                CreatedAt: analysis.CreatedAt
            );
        }

    }
}
