using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Features.Subscriptions.DTOs;
using TradingAI.Domain.Enums;

namespace TradingAI.Application.Features.Subscriptions.Commands.UpgradeSubscription
{

    public record UpgradeSubscriptionCommand(Guid UserId, SubscriptionTier NewTier) : IRequest<SubscriptionDto>;
}
