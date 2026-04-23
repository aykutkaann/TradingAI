using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
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

            var analysis = new Analysis
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                AssetId = asset.Id,
                Pair = asset.Pair,
                TimeFrame = request.TimeFrame,
                ImageUrl = null, 
                Summary = result.Summary,
                TrendDirection = result.TrendDirection,
                DetectedPatterns = JsonSerializer.Serialize(result.DetectedPatterns),
                SuggestedEntry = result.SuggestedEntry,
                StopLoss = result.StopLoss,
                TakeProfit1 = result.TakeProfit1,
                TakeProfit2 = result.TakeProfit2,
                RiskRewardRatio = result.RiskRewardRatio,
                CreatedAt = DateTime.UtcNow
            };

            db.Analyses.Add(analysis);

            user.AnalysisCountThisMonth += 1;

            await db.SaveChangesAsync(cancellationToken);

            return MapToDto(analysis, user, asset);




        }

        private AnalysisDto MapToDto(Analysis analysis, User user, Asset? asset = null)
        {

            var patterns = JsonSerializer.Deserialize<List<string>>(analysis.DetectedPatterns ?? "[]") ?? new List<string>();

            var supportLevels = JsonSerializer.Deserialize<List<decimal>>(analysis.KeyLevels ?? "[]") ?? new List<decimal>();
            var resistanceLevels = JsonSerializer.Deserialize<List<decimal>>(analysis.KeyLevels ?? "[]") ?? new List<decimal>();

            return new AnalysisDto(
                Id: analysis.Id,
                UserId: user.Id,
                UserDisplayName: user.DisplayName ?? user.UserName,
                AssetId: asset?.Id,
                AssetSymbol: asset?.Symbol,
                AssetPair: analysis.Pair, // Entity'deki ham değer
                TimeFrame: analysis.TimeFrame,
                ChartImageUrl: analysis.ImageUrl,
                TrendDirection: analysis.TrendDirection, // "Bullish", "Bearish" vb.
                DetectedPaterns: patterns,
                SupportLevels: supportLevels,
                ResistanceLevels: resistanceLevels,
                SuggestedEntry: analysis.SuggestedEntry,
                StopLoss: analysis.StopLoss,
                TakeProfit1: analysis.TakeProfit1,
                TakeProfit2: analysis.TakeProfit2,
                RiskRewardRatio: analysis.RiskRewardRatio,
                Analysis: analysis.AiAnalysis, // AI'nın detaylı analizi
                Summary: analysis.Summary,     // AI'nın kısa özeti
                IsPublished: analysis.IsPublished,
                LikeCount: analysis.Likes?.Count ?? 0,
                CommentCount: analysis.Comments?.Count ?? 0,
                CreatedAt: DateTime.UtcNow
            );
        }
    }
}
