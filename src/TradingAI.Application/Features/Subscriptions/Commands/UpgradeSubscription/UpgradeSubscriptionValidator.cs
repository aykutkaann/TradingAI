using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.Subscriptions.Commands.UpgradeSubscription
{
    public class UpgradeSubscriptionValidator :AbstractValidator<UpgradeSubscriptionCommand>
    {
        public UpgradeSubscriptionValidator()
        {
            RuleFor(x => x.NewTier).IsInEnum();

            RuleFor(x => x.NewTier).NotEqual(Domain.Enums.SubscriptionTier.Free);
        }
    }
}
