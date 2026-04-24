using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Common.RateLimiting
{
    public class UsageCounter(IApplicationDbContext db) : IUsageCounter
    {
        public  Task<int> CountAnalysesTodayAsync(Guid userId, CancellationToken ct = default)
        {
            var startOfDayUtc = DateTime.UtcNow.Date;

            return db.Analyses
                    .AsNoTracking()
                    .Where(a => a.UserId == userId && a.CreatedAt >= startOfDayUtc)
                    .CountAsync(ct);
        }

        public  Task<int> CountPublishesTodayAsync(Guid userId, CancellationToken ct = default)
        {

            var startOfDayUtc = DateTime.UtcNow.Date;

            return db.Analyses
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.IsPublished && a.PublishedAt != null && a.PublishedAt >= startOfDayUtc).CountAsync(ct);

        }
    }
}
