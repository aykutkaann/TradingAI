using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.RateLimiting;

namespace TradingAI.Application.Features.Analyses.Commands.PublishAnalysis
{

    public record PublishAnalysisCommand(
        Guid AnalysisId,
        Guid CurrentUserId
    ) : IRequest<Unit>, IUsesDailyPublishSlot;
}
