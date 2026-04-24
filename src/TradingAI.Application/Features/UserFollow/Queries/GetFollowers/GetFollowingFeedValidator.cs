using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.UserFollow.Queries.GetFollowers
{
    public class GetFollowingFeedValidator :AbstractValidator<GetFollowersQuery>
    {
        public GetFollowingFeedValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
