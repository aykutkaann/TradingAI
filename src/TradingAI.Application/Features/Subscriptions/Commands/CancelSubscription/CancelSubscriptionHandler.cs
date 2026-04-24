using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Features.Subscriptions.DTOs;

namespace TradingAI.Application.Features.Subscriptions.Commands.CancelSubscription
{
    public class CancelSubscriptionHandler(IApplicationDbContext db) : IRequestHandler<CancelSubscriptionCommand, SubscriptionDto>
    {
        public async Task<SubscriptionDto> Handle(CancelSubscriptionCommand request, CancellationToken ct)
        {
            var currentSub = await db.UserSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.IsActive,ct);

            if (currentSub == null)
                throw new NotFoundException("Subscription not found.");

            if(currentSub.Tier == Domain.Enums.SubscriptionTier.Free)
                throw new ConflictException("You are already using the FREE plan.");

            var freePlan = await db.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Tier == Domain.Enums.SubscriptionTier.Free && p.IsActive,ct);

            if (freePlan == null)
                throw new NotFoundException("Plan not found.");


            currentSub.PlanId = freePlan.Id;
            currentSub.Tier = Domain.Enums.SubscriptionTier.Free;
            currentSub.ExpiresAt = null;
            currentSub.IsActive = true;
            currentSub.CancelledAt = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);


            return new SubscriptionDto(currentSub.Tier, freePlan.Name, freePlan.DailyAnalysisLimit, freePlan.DailyPublishLimit, freePlan.CanUseAssetAnalysis
                , freePlan.CanAccessLeaderBoard, freePlan.MaxImageSizeMb, currentSub.ExpiresAt, true, currentSub.StartedAt);



        }
    }
}
