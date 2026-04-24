using System;
using System.Collections.Generic;
using TradingAI.Application.Common.Models;
using TradingAI.Domain.Entities;
using TradingAI.Domain.Enums;
using TradingAI.Infrastructure.AI;

namespace TradingAI.UnitTests.Outcomes
{
    public class OutcomeEvaluatorTests
    {
        private readonly OutcomeEvaluator _sut = new();

        // ---------- helpers ----------

        private static Analysis MakeBullish(
            decimal entry = 100m,
            decimal sl = 90m,
            decimal tp1 = 110m,
            decimal tp2 = 120m,
            DateTime? expiresAt = null)
        {
            return new Analysis
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                TimeFrame = "1h",
                Pair = "BTC/USDT",
                AiAnalysis = "test",
                TrendDirection = "Bullish",
                SuggestedEntry = entry,
                StopLoss = sl,
                TakeProfit1 = tp1,
                TakeProfit2 = tp2,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ExpiresAt = expiresAt ?? new DateTime(2026, 1, 8, 0, 0, 0, DateTimeKind.Utc),
            };
        }

        private static Analysis MakeBearish(
            decimal entry = 100m,
            decimal sl = 110m,
            decimal tp1 = 90m,
            decimal tp2 = 80m,
            DateTime? expiresAt = null)
        {
            var a = MakeBullish(entry, sl, tp1, tp2, expiresAt);
            a.TrendDirection = "Bearish";
            return a;
        }

        private static PriceCandle Candle(
            int hourOffset, decimal open, decimal high, decimal low, decimal close)
        {
            var baseTime = new DateTime(2026, 1, 1, 1, 0, 0, DateTimeKind.Utc);
            return new PriceCandle(baseTime.AddHours(hourOffset), open, high, low, close);
        }

        private static DateTime Now(int daysAfterCreate = 1)
            => new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(daysAfterCreate);

        // ---------- neutral / invalidated ----------

        [Fact]
        public void Neutral_trend_returns_Invalidated_and_skips_candles()
        {
            var analysis = MakeBullish();
            analysis.TrendDirection = "Neutral";

            // Even with a candle that would hit TP1, neutral must short-circuit.
            var candles = new List<PriceCandle> { Candle(1, 100, 200, 100, 200) };

            var result = _sut.Evaluate(analysis, candles, Now());

            Assert.Equal(AnalysisOutcome.Invalidated, result.Outcome);
            Assert.False(result.TakeProfit1Hit);
            Assert.False(result.TakeProfit2Hit);
            Assert.False(result.StopLossHit);
            Assert.Null(result.ResolvedAt);
            Assert.Null(result.ResolvedPrice);
        }

        // ---------- bullish ----------

        [Fact]
        public void Bullish_TP1_hit_only_is_Win()
        {
            var analysis = MakeBullish();
            var candles = new List<PriceCandle>
            {
                Candle(1, 100, 112, 95, 111),  // TP1 (110) touched, TP2 (120) not, SL (90) not
            };

            var result = _sut.Evaluate(analysis, candles, Now());

            Assert.Equal(AnalysisOutcome.Win, result.Outcome);
            Assert.True(result.TakeProfit1Hit);
            Assert.False(result.TakeProfit2Hit);
            Assert.False(result.StopLossHit);
            Assert.Equal(110m, result.ResolvedPrice);
            Assert.Equal(candles[0].Time, result.ResolvedAt);
        }

        [Fact]
        public void Bullish_TP1_then_TP2_is_Win_and_reports_TP2_resolve_point()
        {
            var analysis = MakeBullish();
            var candles = new List<PriceCandle>
            {
                Candle(1, 100, 112, 98,  111),  // TP1 only
                Candle(2, 111, 122, 110, 121),  // TP2 hit
            };

            var result = _sut.Evaluate(analysis, candles, Now());

            Assert.Equal(AnalysisOutcome.Win, result.Outcome);
            Assert.True(result.TakeProfit1Hit);
            Assert.True(result.TakeProfit2Hit);
            Assert.False(result.StopLossHit);
            Assert.Equal(120m, result.ResolvedPrice);
            Assert.Equal(candles[1].Time, result.ResolvedAt);
        }

        [Fact]
        public void Bullish_SL_hit_first_is_Loss()
        {
            var analysis = MakeBullish();
            var candles = new List<PriceCandle>
            {
                Candle(1, 100, 105, 88, 92),  // SL (90) touched
                Candle(2, 92,  130, 92, 125), // would have hit TP1+TP2 — must not count
            };

            var result = _sut.Evaluate(analysis, candles, Now());

            Assert.Equal(AnalysisOutcome.Loss, result.Outcome);
            Assert.False(result.TakeProfit1Hit);
            Assert.False(result.TakeProfit2Hit);
            Assert.True(result.StopLossHit);
            Assert.Equal(90m, result.ResolvedPrice);
            Assert.Equal(candles[0].Time, result.ResolvedAt);
        }

        [Fact]
        public void Bullish_SL_after_TP1_still_reports_Win_with_TP1_resolve_point()
        {
            // Key correctness check: SL later than TP1 must NOT overwrite the
            // Win's resolved price/time with the SL's.
            var analysis = MakeBullish();
            var candles = new List<PriceCandle>
            {
                Candle(1, 100, 111, 99, 110),  // TP1 hit
                Candle(2, 110, 112, 88, 92),   // SL hit later — still a Win overall
            };

            var result = _sut.Evaluate(analysis, candles, Now());

            Assert.Equal(AnalysisOutcome.Win, result.Outcome);
            Assert.True(result.TakeProfit1Hit);
            Assert.False(result.TakeProfit2Hit);
            Assert.True(result.StopLossHit);
            Assert.Equal(110m, result.ResolvedPrice);       // TP1 price, NOT SL
            Assert.Equal(candles[0].Time, result.ResolvedAt); // TP1 candle, NOT SL
        }

        [Fact]
        public void Bullish_same_candle_SL_and_TP_is_Loss_conservative()
        {
            var analysis = MakeBullish();
            // Candle sweeps both levels — conservative rule: SL wins.
            var candles = new List<PriceCandle>
            {
                Candle(1, 100, 115, 88, 100),
            };

            var result = _sut.Evaluate(analysis, candles, Now());

            Assert.Equal(AnalysisOutcome.Loss, result.Outcome);
            Assert.False(result.TakeProfit1Hit);
            Assert.True(result.StopLossHit);
            Assert.Equal(90m, result.ResolvedPrice);
        }

        [Fact]
        public void Bullish_nothing_hit_before_expiry_is_Pending()
        {
            var analysis = MakeBullish(
                expiresAt: new DateTime(2026, 1, 8, 0, 0, 0, DateTimeKind.Utc));

            var candles = new List<PriceCandle>
            {
                Candle(1, 100, 105, 95, 102), // inside range
                Candle(2, 102, 108, 94, 103),
            };

            // "Now" is before expiry
            var now = new DateTime(2026, 1, 3, 0, 0, 0, DateTimeKind.Utc);

            var result = _sut.Evaluate(analysis, candles, now);

            Assert.Equal(AnalysisOutcome.Pending, result.Outcome);
            Assert.False(result.TakeProfit1Hit);
            Assert.False(result.StopLossHit);
            Assert.Null(result.ResolvedAt);
            Assert.Null(result.ResolvedPrice);
        }

        [Fact]
        public void Bullish_nothing_hit_after_expiry_is_Expired()
        {
            var expires = new DateTime(2026, 1, 8, 0, 0, 0, DateTimeKind.Utc);
            var analysis = MakeBullish(expiresAt: expires);

            var candles = new List<PriceCandle>
            {
                Candle(1, 100, 105, 95, 102),
            };

            var now = expires.AddMinutes(1);

            var result = _sut.Evaluate(analysis, candles, now);

            Assert.Equal(AnalysisOutcome.Expired, result.Outcome);
            Assert.Equal(expires, result.ResolvedAt);
            Assert.Null(result.ResolvedPrice);
        }

        // ---------- bearish ----------

        [Fact]
        public void Bearish_TP1_hit_is_Win()
        {
            var analysis = MakeBearish(); // entry 100, SL 110, TP1 90, TP2 80
            var candles = new List<PriceCandle>
            {
                Candle(1, 100, 102, 89, 91), // dipped to 89 → TP1 (90) touched
            };

            var result = _sut.Evaluate(analysis, candles, Now());

            Assert.Equal(AnalysisOutcome.Win, result.Outcome);
            Assert.True(result.TakeProfit1Hit);
            Assert.False(result.TakeProfit2Hit);
            Assert.False(result.StopLossHit);
            Assert.Equal(90m, result.ResolvedPrice);
        }

        [Fact]
        public void Bearish_SL_hit_is_Loss()
        {
            var analysis = MakeBearish();
            var candles = new List<PriceCandle>
            {
                Candle(1, 100, 111, 99, 108), // spiked to 111 → SL (110) touched
            };

            var result = _sut.Evaluate(analysis, candles, Now());

            Assert.Equal(AnalysisOutcome.Loss, result.Outcome);
            Assert.True(result.StopLossHit);
            Assert.False(result.TakeProfit1Hit);
            Assert.Equal(110m, result.ResolvedPrice);
        }

        // ---------- prior-tick progress ----------

        [Fact]
        public void Seeds_from_prior_tick_flags_so_progress_is_not_lost()
        {
            // Simulate: on a previous tick, TP1 already got recorded.
            // This tick has no TP1-touching candle, but the prior flag must stand
            // so the outcome is still Win.
            var analysis = MakeBullish();
            analysis.TakeProfit1Hit = true;

            var candles = new List<PriceCandle>
            {
                Candle(1, 100, 105, 98, 102), // nothing new hit this tick
            };

            var result = _sut.Evaluate(analysis, candles, Now());

            Assert.Equal(AnalysisOutcome.Win, result.Outcome);
            Assert.True(result.TakeProfit1Hit);
            // Note: tp1At in THIS run is null (the hit was on a prior tick,
            // not in these candles), so resolvedAt is null. This is a known
            // trade-off — persist resolvedAt at the worker layer when you
            // first record the flag.
            Assert.Null(result.ResolvedAt);
        }
    }
}
