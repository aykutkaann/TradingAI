using MediatR;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.RateLimiting;


namespace TradingAI.Application.Common.Behaviors;

public class RateLimitBehavior<TRequest, TResponse>(
    IEntitlementService entitlements,
    IUsageCounter counter,
    ICurrentUserService currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {

        if (request is IUsesDailyAnalysisSlot)
        {
            var userId = currentUser.RequireUserId();   // pipeline only runs on authed commands
            var ent = await entitlements.GetAsync(userId, ct);

            // Free tier: 3 analyses TOTAL (lifetime). Once consumed, the user
            // must upgrade — no daily reset. Paid tiers use the daily window.
            var isFree = ent.Tier == Domain.Enums.SubscriptionTier.Free;
            var used = isFree
                ? await counter.CountAnalysesLifetimeAsync(userId, ct)
                : await counter.CountAnalysesTodayAsync(userId, ct);

            if (used >= ent.DailyAnalysisLimit)
                throw new RateLimitExceededException("analysis", used, ent.DailyAnalysisLimit);
        }

        if (request is IUsesDailyPublishSlot)
        {

            var userId = currentUser.RequireUserId();   
            var ent = await entitlements.GetAsync(userId, ct);
            var used = await counter.CountPublishesTodayAsync(userId, ct);
            if (used >= ent.DailyPublishLimit)
                throw new RateLimitExceededException("publish", used, ent.DailyPublishLimit);
        }

        return await next(ct);
    }
}