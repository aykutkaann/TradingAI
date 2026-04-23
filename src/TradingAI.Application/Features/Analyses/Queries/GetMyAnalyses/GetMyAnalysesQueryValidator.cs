using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.Analyses.Queries.GetMyAnalyses
{
    public class GetMyAnalysesQueryValidator :AbstractValidator<GetMyAnalysesQuery>
    {
        public GetMyAnalysesQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
