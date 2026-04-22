using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Models;
using TradingAI.Domain.Entities;

namespace TradingAI.Application.Common.Interfaces
{
    public interface IMarketDataService
    {
        Task<PriceData?> GetLivePriceAsync(Asset asset, CancellationToken ct);
        Task<IReadOnlyList<PriceData>> GetBatchPriceAsync(IReadOnlyList<Asset> assets, CancellationToken ct);
        Task<IReadOnlyList<OhlcCandle>> GetHistoricalDataAsync(Asset asset, string interval, int limit, CancellationToken ct);

    }
}
