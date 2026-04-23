using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.Analyses.Dtos;
using TradingAI.Domain.Entities;

namespace TradingAI.Application.Features.Analyses.Queries.GetMyAnalyses
{
    public class GetMyAnalysesQueryHandler(IApplicationDbContext db) : IRequestHandler<GetMyAnalysesQuery, PagedResult<AnalysisDto>>
    {


        public async Task<PagedResult<AnalysisDto>> Handle(GetMyAnalysesQuery request, CancellationToken ct)
        {
            var query = db.Analyses.AsNoTracking().Where(a => a.UserId == request.UserId).OrderByDescending(a => a.CreatedAt);

            var total = await query.CountAsync(ct);

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(a => a.Asset)
                .Include(a => a.User)
                .ToListAsync(ct);

            var dtos = items.Select(a => MapToDto(a, a.User, a.Asset)).ToList();

            return new PagedResult<AnalysisDto>(dtos, total, request.Page, request.PageSize);

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
