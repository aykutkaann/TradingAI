using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.Analyses.Commands.UnlikeAnalysis
{
    public record UnlikeAnalysisCommand(Guid AnalysisId, Guid CurrentUserId) : IRequest<Unit>;
    
    
}
