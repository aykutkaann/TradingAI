using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.Analyses.Commands.AnalyzeAsset;
using TradingAI.Application.Features.Analyses.Dtos;
using TradingAI.Domain.Entities;
using TradingAI.Domain.Enums;

namespace TradingAI.Application.Features.Analyses.Commands.AnalyzeImage
{
    public class AnalyzeImageCommandHandler(
        IApplicationDbContext db,
        IAiAnalysisService ai,
        IFileStorage fileStorage) : IRequestHandler<AnalyzeImageCommand, AnalysisDto>
    {

        public async Task<AnalysisDto> Handle(AnalyzeImageCommand request, CancellationToken cancellationToken)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken) 
                ?? throw new UnauthorizedException("Invalid credentials.");


            //Analys counter reset
            var now = DateTime.UtcNow;
            if(user.LastAnalysisResetDate.Year != now.Year || user.LastAnalysisResetDate.Month != now.Month)
            {
                user.AnalysisCountThisMonth = 0;
                user.LastAnalysisResetDate = now;
            }

            //cap check

            var cap = SubscriptionLimits.MonthlyAnalysisCap(user.Role);
            if(user.AnalysisCountThisMonth >= cap)
                throw new ConflictException($"Monthly analysis limit ({cap}) reached for {user.Role} plan.");



            using var buffered = new MemoryStream();
            await request.ImageStream.CopyToAsync(buffered, cancellationToken);
            buffered.Position = 0;

            //call AI
            var result = await ai.AnalyzeChartImageAsync(buffered, request.ImageMediaType, request.AssetPair, request.TimeFrame, request.UserPrompt
                , cancellationToken);

            result = AiAnalysisValidator.SanitizeTradeLevels(result);

            buffered.Position = 0;
            var imageUrl = await fileStorage.SaveAsync(buffered, request.FileName, user.Id.ToString(), cancellationToken);



            var analysis = new Analysis
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                AssetId = null,                     // ← see issue #3 below
                Type = AnalysisType.ImageUpload,
                ImageUrl = imageUrl,
                UserPrompt = request.UserPrompt,
                TimeFrame = request.TimeFrame,
                Pair = request.AssetPair,           // free-text user-entered pair
                AiAnalysis = result.Analysis,
                TrendDirection = result.TrendDirection,
                Summary = result.Summary,
                DetectedPatterns = JsonSerializer.Serialize(result.DetectedPatterns),
                KeyLevels = JsonSerializer.Serialize(result.KeyLevels),   // whole object
                SuggestedEntry = result.SuggestedEntry,
                StopLoss = result.StopLoss,
                TakeProfit1 = result.TakeProfit1,
                TakeProfit2 = result.TakeProfit2,
                RiskRewardRatio = result.RiskRewardRatio,
                IsPublished = false,
                CreatedAt = DateTime.UtcNow
            };

            db.Analyses.Add(analysis);

            user.AnalysisCountThisMonth += 1;
            await db.SaveChangesAsync(cancellationToken);

            return MapToDto(analysis, user);
            

        }


        private sealed record KeyLevelsJson(List<decimal>? Support, List<decimal>? Resistance);

        private AnalysisDto MapToDto(Analysis analysis, User user, Asset? asset = null)
        {
  
            var patterns = JsonSerializer.Deserialize<List<string>>(analysis.DetectedPatterns ?? "[]") ?? new List<string>();


            var keyLevels = string.IsNullOrEmpty(analysis.KeyLevels)
                ? new KeyLevelsJson(null, null)
                : JsonSerializer.Deserialize<KeyLevelsJson>(analysis.KeyLevels,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                  ?? new KeyLevelsJson(null, null);

            var supportLevels = keyLevels.Support ?? new List<decimal>();
            var resistanceLevels = keyLevels.Resistance ?? new List<decimal>();

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
                CreatedAt: analysis.CreatedAt
            );
        }
    } 

    
}
