using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Features.Subscriptions.DTOs;

namespace TradingAI.Application.Features.Subscriptions.Queries.GetMySubscription
{

    public record GetMySubscriptionQuery(Guid UserId) : IRequest<SubscriptionDto>;
}
