using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Features.Subscriptions.DTOs;

namespace TradingAI.Application.Features.Subscriptions.Queries.GetMySubscription
{
    public class GetMySubscriptionHandler(IApplicationDbContext db) : IRequestHandler<GetMySubscriptionQuery, SubscriptionDto>
    {
        public async Task<SubscriptionDto> Handle(GetMySubscriptionQuery request, CancellationToken ct)
        {

            var sub = await db.UserSubscriptions
                .AsNoTracking()
                .Where(s => s.UserId == request.UserId && s.IsActive)
                .OrderByDescending(s => s.StartedAt)
                .Select(s => new SubscriptionDto(
                    s.Tier,
                    s.Plan.Name,
                    s.Plan.DailyAnalysisLimit,
                    s.Plan.DailyPublishLimit,
                    s.Plan.CanUseAssetAnalysis,
                    s.Plan.CanAccessLeaderBoard,
                    s.Plan.MaxImageSizeMb,
                    s.ExpiresAt,
                    s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow,
                    s.StartedAt)).FirstOrDefaultAsync(ct);

            if (sub == null)
                throw new NotFoundException("Subscription not found.");

            return sub;


        }
    }
}
