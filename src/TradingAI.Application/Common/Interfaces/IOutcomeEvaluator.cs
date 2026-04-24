using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.Analyses.Outcomes;
using TradingAI.Domain.Entities;

namespace TradingAI.Application.Common.Interfaces
{
    public interface IOutcomeEvaluator
    {
        EvaluationResult Evaluate(Analysis analysis, IReadOnlyList<PriceCandle> candles, DateTime nowUtc);
    }
}
