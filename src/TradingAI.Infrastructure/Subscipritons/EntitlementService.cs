using Microsoft.EntityFrameworkCore;

using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Features.Subscriptions.Models;
using TradingAI.Domain.Enums;

namespace TradingAI.Infrastructure.Subscipritons
{

    public class EntitlementService(IApplicationDbContext db) : IEntitlementService
    {
        public async Task<Entitlements> GetAsync(Guid userId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            var ent = await (from s in db.UserSubscriptions.AsNoTracking()
                             join p in db.SubscriptionPlans.AsNoTracking() on s.Tier equals p.Tier
                             where s.UserId == userId
                                   && s.IsActive
                                   && (s.ExpiresAt == null || s.ExpiresAt > now)
                             select new Entitlements(
                                 p.Tier,
                                 p.DailyAnalysisLimit,
                                 p.DailyPublishLimit,
                                 p.CanUseAssetAnalysis,
                                 p.CanAccessLeaderBoard,
                                 p.CanSeePlatformStats,
                                 p.MaxImageSizeMb,
                                 s.ExpiresAt))
                            .FirstOrDefaultAsync(ct);

            if (ent is not null) return ent;

            var free = await db.SubscriptionPlans.AsNoTracking()
                .Where(p => p.Tier == SubscriptionTier.Free)
                .Select(p => new Entitlements(
                    p.Tier,
                    p.DailyAnalysisLimit,
                    p.DailyPublishLimit,
                    p.CanUseAssetAnalysis,
                    p.CanAccessLeaderBoard,
                    p.CanSeePlatformStats,
                    p.MaxImageSizeMb,
                    null))
                .FirstOrDefaultAsync(ct);

            if (free is null)
                throw new InvalidOperationException("Free plan not seeded. Run PlanSeeder.");

            return free;
        }
    }
}
