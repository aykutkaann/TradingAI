using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Features.Analyses.Dtos;
using TradingAI.Domain.Entities;
using TradingAI.Domain.Enums;

namespace TradingAI.Application.Features.Analyses.Commands.AnalyzeAsset
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


            buffered.Position = 0;
            var imageUrl = await fileStorage.SaveAsync(buffered, request.FileName, user.Id.ToString(), cancellationToken);


             var analysis = new Analysis
             {
                 Id = Guid.NewGuid(),
                 UserId = user.Id,
                 Type = AnalysisType.ImageUpload,
                 ImageUrl = request.ImageMediaType,
                 UserPrompt = request.UserPrompt,
                 TimeFrame = request.TimeFrame
             };

            db.Analyses.Add(analysis);

            user.AnalysisCountThisMonth += 1;
            await db.SaveChangesAsync(cancellationToken);

            return MapToDto(analysis, user);
            

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
