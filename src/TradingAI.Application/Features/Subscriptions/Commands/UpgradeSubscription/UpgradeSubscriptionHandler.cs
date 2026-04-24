using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Features.Subscriptions.DTOs;

namespace TradingAI.Application.Features.Subscriptions.Commands.UpgradeSubscription
{
    public class UpgradeSubscriptionHandler(IApplicationDbContext db) : IRequestHandler<UpgradeSubscriptionCommand, SubscriptionDto>
    {
        public async Task<SubscriptionDto> Handle(UpgradeSubscriptionCommand request, CancellationToken ct)
        {
            var currentSub = await db.UserSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.IsActive, ct);

            if (currentSub == null)
                throw new NotFoundException("No active subscription.");

            if (currentSub.Tier == request.NewTier)
                throw new ConflictException($"You are already using the {request.NewTier} plan.");

            // Block downgrades — cancel flow handles Free, separate command would handle Premium -> Pro later.
            if ((int)request.NewTier < (int)currentSub.Tier)
                throw new ConflictException("Cannot downgrade via upgrade endpoint.");

            var targetPlan = await db.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Tier == request.NewTier && p.IsActive, ct);

            if (targetPlan == null)
                throw new NotFoundException($"Plan for tier '{request.NewTier}' is not configured.");

            var now = DateTime.UtcNow;

            currentSub.PlanId = targetPlan.Id;
            currentSub.Tier = targetPlan.Tier;
            currentSub.ExpiresAt = now.AddDays(30);
            currentSub.IsActive = true;
            currentSub.CancelledAt = null;
            // NOTE: StartedAt is intentionally preserved — it marks when the user first subscribed.

            await db.SaveChangesAsync(ct);

            return new SubscriptionDto(
                currentSub.Tier,
                targetPlan.Name,
                targetPlan.DailyAnalysisLimit,
                targetPlan.DailyPublishLimit,
                targetPlan.CanUseAssetAnalysis,
                targetPlan.CanAccessLeaderBoard,
                targetPlan.MaxImageSizeMb,
                currentSub.ExpiresAt,
                currentSub.ExpiresAt == null || currentSub.ExpiresAt > now,
                currentSub.StartedAt);
        }
    }
}
