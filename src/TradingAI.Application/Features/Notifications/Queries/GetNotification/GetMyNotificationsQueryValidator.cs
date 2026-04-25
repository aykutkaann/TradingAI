using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.Notifications.Queries.GetNotification
{

    public class GetMyNotificationsQueryValidator : AbstractValidator<GetNotificationsQuery>
    {
        public GetMyNotificationsQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50);
        }
    }
}
