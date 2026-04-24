using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.Analyses.Commands.AnalyzeImage;
using TradingAI.Application.Features.Analyses.Dtos;
using TradingAI.Domain.Entities;
using TradingAI.Domain.Enums;


namespace TradingAI.Application.Features.Analyses.Commands.AnalyzeAsset
{
    public class AnalyzeAssetCommandHandler(
        IApplicationDbContext db,
        IAiAnalysisService ai,
        IMarketDataService marketData) :IRequestHandler<AnalyzeAssetCommand, AnalysisDto>
    {

        public async Task<AnalysisDto> Handle(AnalyzeAssetCommand request, CancellationToken cancellationToken)
        {
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken)
                ?? throw new NotFoundException("User not found.");

            var now = DateTime.UtcNow;
            if(user.LastAnalysisResetDate.Year != now.Year || user.LastAnalysisResetDate.Month != now.Month)
            {
                user.AnalysisCountThisMonth = 0;
                user.LastAnalysisResetDate = now;
            }

            var cap = SubscriptionLimits.MonthlyAnalysisCap(user.Role);
            if(user.AnalysisCountThisMonth >= cap)
                throw new ConflictException($"Monthly analysis limit ({cap}) reached for {user.Role} plan.");

            var asset = await db.Assets.FindAsync(new object?[] { request.AssetId }, cancellationToken)
                ?? throw new NotFoundException("Asset not found.");

            var livePrice = await marketData.GetLivePriceAsync(asset, cancellationToken);
            if (livePrice == null)
                throw new NotFoundException("Could not fetch live price data.");
            var candles = await marketData.GetHistoricalDataAsync(asset, request.TimeFrame, limit: 100, cancellationToken);

            if (livePrice == null)
                throw new ConflictException("Could not fetch live price data.");

            var result = await ai.AnalyzeAssetAsync(asset.Pair, request.TimeFrame, livePrice.CurrentPrice, candles, request.UserPrompt, cancellationToken);
            result = AiAnalysisValidator.SanitizeTradeLevels(result);
            var analysis = new Analysis
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                AssetId = asset.Id,
                Type = AnalysisType.AssetRequest,              
                Pair = asset.Pair,
                TimeFrame = request.TimeFrame,
                ImageUrl = null,
                UserPrompt = request.UserPrompt,
                AiAnalysis = result.Analysis,                 
                Summary = result.Summary,
                TrendDirection = result.TrendDirection,
                DetectedPatterns = JsonSerializer.Serialize(result.DetectedPatterns),
                KeyLevels = JsonSerializer.Serialize(result.KeyLevels),   
                SuggestedEntry = result.SuggestedEntry,
                StopLoss = result.StopLoss,
                TakeProfit1 = result.TakeProfit1,
                TakeProfit2 = result.TakeProfit2,
                RiskRewardRatio = result.RiskRewardRatio,
                PriceAtAnalysis = livePrice.CurrentPrice,      
                IsPublished = false,
                CreatedAt = DateTime.UtcNow
            };

            db.Analyses.Add(analysis);

            user.AnalysisCountThisMonth += 1;

            await db.SaveChangesAsync(cancellationToken);

            return MapToDto(analysis, user, asset);




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
                Outcome: analysis.Outcome,
                TP1Hit: analysis.TakeProfit1Hit,
                TP2Hit: analysis.TakeProfit2Hit,
                SLHit: analysis.StopLossHit,
                ResolvedPrice: analysis.ResolvedPrice,
                ResolvedAt: analysis.ResolvedAt,
                ExpiresAt: analysis.ExpiresAt,
                CreatedAt: analysis.CreatedAt
            );
        }
    }
}
