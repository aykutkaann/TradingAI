using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.Analyses.Queries.GetPublicFeed
{
    public class GetPublicFeedQueryValidator :AbstractValidator<GetPublicFeedQuery>
    {
        public GetPublicFeedQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);

            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
