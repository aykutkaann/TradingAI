using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.Analyses.Commands.LikeAnalysis
{

    public record LikeAnalysisCommand(
        Guid AnalysisId,
        Guid CurrentUserId
    ) : IRequest<Unit>;
}
