using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Features.Analyses.Dtos;

namespace TradingAI.Application.Features.Analyses.Queries.GetAnalysisById
{

    public record GetAnalysisByIdQuery(Guid AnalysisId, Guid? CurrentUserId) : IRequest<AnalysisDto>;
}
