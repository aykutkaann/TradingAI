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
            if (await db.SubscriptionPlans.AnyAsync(ct)) return;

            var plans = new List<SubscriptionPlan>
            {
             //1-FREE
                new ()
                {
                    Id = Guid.NewGuid(),
                    Tier = Domain.Enums.SubscriptionTier.Free,
                    Name = "Free",
                    PriceWeeklyUsd = 0,
                    PriceMonthlyUsd =0,
                    PriceYearlyUsd = 0,
                    DailyAnalysisLimit = 3,
                    DailyPublishLimit = 1,
                    CanUseAssetAnalysis = false,
                    CanAccessLeaderBoard = false,
                    CanSeePlatformStats = true,
                    MaxImageSizeMb = 2

                },

                //PRO
                new ()
                {
                    Id = Guid.NewGuid(),
                    Tier = Domain.Enums.SubscriptionTier.Pro,
                    Name = "Pro",
                    PriceWeeklyUsd = 3,
                    PriceMonthlyUsd = 10,
                    PriceYearlyUsd = 90,
                    DailyAnalysisLimit = 50,
                    DailyPublishLimit = 20,
                    CanUseAssetAnalysis = true,
                    CanAccessLeaderBoard = true,
                    CanSeePlatformStats = true,
                    MaxImageSizeMb = 5
                },

                //PREMIUM
                new ()
                {
                   
                    Id = Guid.NewGuid(),
                    Tier = Domain.Enums.SubscriptionTier.Premium,
                    Name = "Premium",
                    PriceWeeklyUsd = 14,
                    PriceMonthlyUsd =49,
                    PriceYearlyUsd = 449,
                    DailyAnalysisLimit = 500,
                    DailyPublishLimit = 200,
                    CanUseAssetAnalysis = true,
                    CanAccessLeaderBoard = true,
                    CanSeePlatformStats = true,
                    MaxImageSizeMb = 10
                }

            };

            await db.SubscriptionPlans.AddRangeAsync(plans, ct);
            await db.SaveChangesAsync(ct);



            var userWithoutSub = await db.Users.Where(u => !db.UserSubscriptions.Any(s => s.UserId == u.Id)).ToListAsync(ct);

            foreach (var user in userWithoutSub)
            {
                db.UserSubscriptions.Add(new UserSubscription
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
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
