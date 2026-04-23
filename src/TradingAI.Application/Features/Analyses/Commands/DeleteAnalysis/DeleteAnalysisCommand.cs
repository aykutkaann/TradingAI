using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.Analyses.Commands.DeleteAnalysis
{
    public record DeleteAnalysisCommand(Guid AnalysisId, Guid CurrentUserId) : IRequest<Unit>; // for void handler
    
    
}
