using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Features.Subscriptions.DTOs;

namespace TradingAI.Application.Features.Subscriptions.Commands.CancelSubscription
{

    public record CancelSubscriptionCommand(Guid UserId) : IRequest<SubscriptionDto>;
}
