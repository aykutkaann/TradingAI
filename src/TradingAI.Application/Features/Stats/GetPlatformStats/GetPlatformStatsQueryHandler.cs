using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Features.Stats.DTOs;
using TradingAI.Application.Features.Users.DTOs;

namespace TradingAI.Application.Features.Stats.GetPlatformStats
{
    public class GetPlatformStatsQueryHandler(IApplicationDbContext db) :IRequestHandler<GetPlatformStatsQuery ,PlatformStatsDto>
    {
        public async Task<PlatformStatsDto> Handle(GetPlatformStatsQuery request ,CancellationToken ct)
        {

            var stats = await db.Analyses
                .AsNoTracking()
                .GroupBy(a => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Published = g.Count(a => a.IsPublished),
                    Wins = g.Count(a => a.Outcome == Domain.Enums.AnalysisOutcome.Win),
                    Losses = g.Count(a => a.Outcome == Domain.Enums.AnalysisOutcome.Loss),
                    Expired = g.Count(a => a.Outcome == Domain.Enums.AnalysisOutcome.Expired),
                    Pending = g.Count(a => a.Outcome == Domain.Enums.AnalysisOutcome.Pending),
                    Tp2Hits = g.Count(a => a.TakeProfit2Hit)
                }).FirstOrDefaultAsync(ct);

            if (stats == null)
                return new PlatformStatsDto(0, 0, 0, 0, 0, 0, 0, 0, 0);

            var resolvedCount = stats.Wins + stats.Losses;

            double winRate = resolvedCount == 0 ? 0 : Math.Round((double)stats.Wins / resolvedCount * 100, 2);

            double tp2HitRate = stats.Wins == 0 ? 0 : Math.Round((double)stats.Tp2Hits / stats.Wins * 100, 2);

            return new PlatformStatsDto(
                TotalAnalyses: stats.Total,
                PublishedAnalyses: stats.Published,
                Wins: stats.Wins,
                Losses: stats.Losses,
                Expired: stats.Expired,
                Pending: stats.Pending,
                WinRate: winRate,
                Tp2HitCount: stats.Tp2Hits,
                Tp2HitRate: tp2HitRate
            );
        }

    }
}
