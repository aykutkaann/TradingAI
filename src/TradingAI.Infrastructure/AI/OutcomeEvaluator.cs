using System;
using System.Collections.Generic;
using System.Linq;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.Analyses.Outcomes;
using TradingAI.Domain.Entities;
using TradingAI.Domain.Enums;

namespace TradingAI.Infrastructure.AI
{
    public class OutcomeEvaluator : IOutcomeEvaluator
    {
        public EvaluationResult Evaluate(Analysis analysis, IReadOnlyList<PriceCandle> candles, DateTime nowUtc)
        {
            // Neutral calls are not tradable — mark Invalidated and never scan.
            if (string.Equals(analysis.TrendDirection, "neutral", StringComparison.OrdinalIgnoreCase))
            {
                return new EvaluationResult(
                    Outcome: AnalysisOutcome.Invalidated,
                    TakeProfit1Hit: false,
                    TakeProfit2Hit: false,
                    StopLossHit: false,
                    ResolvedPrice: null,
                    ResolvedAt: null);
            }

            // Seed from prior ticks so progress is not lost across worker runs.
            bool tp1Hit = analysis.TakeProfit1Hit;
            bool tp2Hit = analysis.TakeProfit2Hit;
            bool slHit = analysis.StopLossHit;

           
            DateTime? tp1At = null;
            DateTime? tp2At = null;
            DateTime? slAt = null;

            bool isBullish = string.Equals(analysis.TrendDirection, "bullish", StringComparison.OrdinalIgnoreCase);

            foreach (var candle in candles.OrderBy(c => c.Time))
            {
                if (isBullish)
                {
          
                    if (analysis.StopLoss.HasValue && candle.Low <= analysis.StopLoss.Value)
                    {
                        slHit = true;
                        slAt = candle.Time;
                        break;
                    }

                    if (analysis.TakeProfit1.HasValue && candle.High >= analysis.TakeProfit1.Value && !tp1Hit)
                    {
                        tp1Hit = true;
                        tp1At = candle.Time;
                    }

                    if (analysis.TakeProfit2.HasValue && candle.High >= analysis.TakeProfit2.Value && !tp2Hit)
                    {
                        tp2Hit = true;
                        tp2At = candle.Time;
                    }
                }
                else // Bearish
                {
                    if (analysis.StopLoss.HasValue && candle.High >= analysis.StopLoss.Value)
                    {
                        slHit = true;
                        slAt = candle.Time;
                        break;
                    }

                    if (analysis.TakeProfit1.HasValue && candle.Low <= analysis.TakeProfit1.Value && !tp1Hit)
                    {
                        tp1Hit = true;
                        tp1At = candle.Time;
                    }

                    if (analysis.TakeProfit2.HasValue && candle.Low <= analysis.TakeProfit2.Value && !tp2Hit)
                    {
                        tp2Hit = true;
                        tp2At = candle.Time;
                    }
                }
            }

            // Decide final outcome.
            var status = AnalysisOutcome.Pending;

            if (slHit && !tp1Hit)
                status = AnalysisOutcome.Loss;
            else if (tp1Hit || tp2Hit)
                status = AnalysisOutcome.Win;
            else if (analysis.ExpiresAt.HasValue && nowUtc >= analysis.ExpiresAt.Value)
                status = AnalysisOutcome.Expired;

            DateTime? resolvedAt = null;
            decimal? resolvedPrice = null;

            switch (status)
            {
                case AnalysisOutcome.Win:
                    // Prefer TP2 if it hit, else TP1.
                    resolvedAt = tp2Hit ? tp2At : tp1At;
                    resolvedPrice = tp2Hit ? analysis.TakeProfit2 : analysis.TakeProfit1;
                    break;

                case AnalysisOutcome.Loss:
                    resolvedAt = slAt;
                    resolvedPrice = analysis.StopLoss;
                    break;

                case AnalysisOutcome.Expired:
                    resolvedAt = analysis.ExpiresAt;
                    resolvedPrice = null; // no trigger price — just time ran out
                    break;

                // Pending / Invalidated → leave both null
            }

            return new EvaluationResult(
                Outcome: status,
                TakeProfit1Hit: tp1Hit,
                TakeProfit2Hit: tp2Hit,
                StopLossHit: slHit,
                ResolvedPrice: resolvedPrice,
                ResolvedAt: resolvedAt);
        }
    }
}
