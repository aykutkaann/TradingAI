using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Models;

namespace TradingAI.Application.Common.Interfaces
{
    public interface IAiAnalysisService
    {
        Task<AiAnalysisResult> AnalyzeChartImageAsync(Stream imageStream, string imageMediaType,
            string assetPair, string timeFrame, string? userPrompt, CancellationToken ct);

        Task<AiAnalysisResult> AnalyzeAssetAsync(string assetPair, string timeFrame,
            decimal currentPrice, IReadOnlyList<OhlcCandle> candles, string? userPrompt, CancellationToken ct);
    }
}
