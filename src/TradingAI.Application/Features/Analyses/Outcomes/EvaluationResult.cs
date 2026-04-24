using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Enums;

namespace TradingAI.Application.Features.Analyses.Outcomes
{

    public record EvaluationResult(AnalysisOutcome Outcome, bool TakeProfit1Hit, bool TakeProfit2Hit, bool StopLossHit, 
        decimal? ResolvedPrice, DateTime? ResolvedAt);
}
