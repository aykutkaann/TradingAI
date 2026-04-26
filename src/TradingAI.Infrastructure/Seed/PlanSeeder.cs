using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Domain.Entities;

namespace TradingAI.Infrastructure.Seed
{
    public static class PlanSeeder
    {
        public static async Task SeedPlanAsync(IApplicationDbContext db, CancellationToken ct = default)
        {
            // Upsert — keep this method runnable on every boot so price/limit
            // tweaks land without manual SQL. Lookup by Tier (not Id) so the
            // existing rows keep their PKs and FKs from UserSubscriptions.
            var plans = new List<SubscriptionPlan>
            {
                // FREE — 3 analyses TOTAL (interpreted as lifetime by RateLimitBehavior).
                // Once consumed, the user is sent to /plans.
                new ()
                {
                    Id = Guid.NewGuid(),
                    Tier = Domain.Enums.SubscriptionTier.Free,
                    Name = "Free",
                    PriceWeeklyUsd = 0,
                    PriceMonthlyUsd = 0,
                    PriceYearlyUsd = 0,
                    DailyAnalysisLimit = 3, // lifetime cap for Free tier
                    DailyPublishLimit = 1,
                    CanUseAssetAnalysis = false,
                    CanAccessLeaderBoard = false,
                    CanSeePlatformStats = true,
                    MaxImageSizeMb = 2
                },

                // PRO — $3/week or $9.99/month. 30 analyses/day.
                new ()
                {
                    Id = Guid.NewGuid(),
                    Tier = Domain.Enums.SubscriptionTier.Pro,
                    Name = "Pro",
                    PriceWeeklyUsd = 3.00m,
                    PriceMonthlyUsd = 9.99m,
                    PriceYearlyUsd = 99.00m,
                    DailyAnalysisLimit = 30,
                    DailyPublishLimit = 20,
                    CanUseAssetAnalysis = true,
                    CanAccessLeaderBoard = true,
                    CanSeePlatformStats = true,
                    MaxImageSizeMb = 10
                },

                // PREMIUM — $29.99/month. Unlimited analyses (large cap so we never block).
                new ()
                {
                    Id = Guid.NewGuid(),
                    Tier = Domain.Enums.SubscriptionTier.Premium,
                    Name = "Premium",
                    PriceWeeklyUsd = 0m, // monthly-only billing for premium
                    PriceMonthlyUsd = 29.99m,
                    PriceYearlyUsd = 299.00m,
                    DailyAnalysisLimit = 9999, // effectively unlimited
                    DailyPublishLimit = 200,
                    CanUseAssetAnalysis = true,
                    CanAccessLeaderBoard = true,
                    CanSeePlatformStats = true,
                    MaxImageSizeMb = 25
                }
            };

            // Upsert each plan by Tier.
            var existingByTier = await db.SubscriptionPlans
                .ToDictionaryAsync(p => p.Tier, ct);

            foreach (var p in plans)
            {
                if (existingByTier.TryGetValue(p.Tier, out var existing))
                {
                    existing.Name = p.Name;
                    existing.PriceWeeklyUsd = p.PriceWeeklyUsd;
                    existing.PriceMonthlyUsd = p.PriceMonthlyUsd;
                    existing.PriceYearlyUsd = p.PriceYearlyUsd;
                    existing.DailyAnalysisLimit = p.DailyAnalysisLimit;
                    existing.DailyPublishLimit = p.DailyPublishLimit;
                    existing.CanUseAssetAnalysis = p.CanUseAssetAnalysis;
                    existing.CanAccessLeaderBoard = p.CanAccessLeaderBoard;
                    existing.CanSeePlatformStats = p.CanSeePlatformStats;
                    existing.MaxImageSizeMb = p.MaxImageSizeMb;
                }
                else
                {
                    db.SubscriptionPlans.Add(p);
                }
            }

            await db.SaveChangesAsync(ct);

            // After upsert, re-read the Free plan id (it may be the existing one).
            var freePlanId = await db.SubscriptionPlans
                .Where(p => p.Tier == Domain.Enums.SubscriptionTier.Free)
                .Select(p => p.Id)
                .FirstAsync(ct);

            var userWithoutSub = await db.Users.Where(u => !db.UserSubscriptions.Any(s => s.UserId == u.Id)).ToListAsync(ct);

            foreach (var user in userWithoutSub)
            {
                db.UserSubscriptions.Add(new UserSubscription
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    PlanId = freePlanId,
                    Tier = Domain.Enums.SubscriptionTier.Free,
                    StartedAt = DateTime.UtcNow,
                    IsActive = true,
                    ExpiresAt = null

                });

            }

            

            await db.SaveChangesAsync(ct);


        }
    }
}
