using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.Analyses.Dtos;

namespace TradingAI.Application.Features.Analyses.Queries.GetMyAnalyses
{

    public record GetMyAnalysesQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<AnalysisDto>>;
}
