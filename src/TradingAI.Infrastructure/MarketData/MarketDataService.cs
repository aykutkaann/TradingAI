using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.Models;
using TradingAI.Domain.Entities;
using TradingAI.Domain.Enums;

namespace TradingAI.Infrastructure.MarketData;

public class MarketDataService : IMarketDataService
{
    private readonly CoinGeckoClient _coinGecko;
    private readonly TwelveDataClient _twelveData;
    private readonly ICacheService _cache;
    private readonly IApplicationDbContext _db;

    private static readonly TimeSpan LivePriceTtl = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan HistoryTtl   = TimeSpan.FromMinutes(5);

    // Worker calls this for outcome tracking. We pull a generous slice of
    // recent history so even older analyses get evaluated against enough candles.
    private const int CandleFetchLimit = 500;

    public MarketDataService(CoinGeckoClient coinGecko, TwelveDataClient twelveData, ICacheService cache, IApplicationDbContext db)
    {
        _coinGecko = coinGecko;
        _twelveData = twelveData;
        _cache = cache;
        _db = db;
    }

    public async Task<PriceData?> GetLivePriceAsync(Asset asset, CancellationToken ct)
    {
        var key = $"price:{asset.Pair}";
        var cached = await _cache.GetAsync<PriceData>(key, ct);
        if (cached is not null) return cached;

        // Protect callers from external provider hangs/failures: use short per-call timeouts
        // and treat failures as missing price (null). Preserve cancellation if caller cancels.
        var fresh = asset.Type switch
        {
            AssetType.Crypto => await SafeSingleCall(ct, TimeSpan.FromSeconds(2), ct2 => _coinGecko.GetLivePriceAsync(asset, ct2)),
            AssetType.Forex or AssetType.Commodity or AssetType.Stock => await SafeSingleCall(ct, TimeSpan.FromSeconds(2), ct2 => _twelveData.GetLivePriceAsync(asset, ct2)),
            _ => null
        };

        if (fresh is not null)
            await _cache.SetAsync(key, fresh, LivePriceTtl, ct);

        return fresh;
    }

    // Helper to run a single-item provider call with a short timeout and safe error handling.
    private static async Task<PriceData?> SafeSingleCall(CancellationToken ambientCt, TimeSpan timeout, Func<CancellationToken, Task<PriceData?>> call)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ambientCt);
        cts.CancelAfter(timeout);
        try
        {
            return await call(cts.Token);
        }
        catch (OperationCanceledException) when (!ambientCt.IsCancellationRequested)
        {
            // provider timeout — treat as missing price
            return null;
        }
        catch
        {
            return null;
        }
    }

    // Helper for batch provider calls. Same semantics as SafeSingleCall.
    private static async Task<IReadOnlyList<PriceData>> SafeBatchCall(CancellationToken ambientCt, TimeSpan timeout, Func<CancellationToken, Task<IReadOnlyList<PriceData>>> call)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ambientCt);
        cts.CancelAfter(timeout);
        try
        {
            var r = await call(cts.Token);
            return r ?? Array.Empty<PriceData>();
        }
        catch (OperationCanceledException) when (!ambientCt.IsCancellationRequested)
        {
            return Array.Empty<PriceData>();
        }
        catch
        {
            return Array.Empty<PriceData>();
        }
    }

    public async Task<IReadOnlyList<PriceData>> GetBatchPriceAsync(IReadOnlyList<Asset> assets, CancellationToken ct)
    {
        if (assets.Count == 0) return Array.Empty<PriceData>();

        var result = new List<PriceData>();
        var missingCrypto = new List<Asset>();
        var missingOther  = new List<Asset>();

        // 1) try cache per asset
        foreach (var asset in assets)
        {
            var cached = await _cache.GetAsync<PriceData>($"price:{asset.Pair}", ct);
            if (cached is not null)
                result.Add(cached);
            else if (asset.Type == AssetType.Crypto)
                missingCrypto.Add(asset);
            else
                missingOther.Add(asset);
        }

        // 2) batch-fetch the misses per provider
        if (missingCrypto.Count > 0)
        {
            var fresh = await SafeBatchCall(ct, TimeSpan.FromSeconds(2), ct2 => _coinGecko.GetBatchPricesAsync(missingCrypto, ct2));
            foreach (var p in fresh)
            {
                await _cache.SetAsync($"price:{p.Symbol}", p, LivePriceTtl, ct);
                result.Add(p);
            }
        }

        if (missingOther.Count > 0)
        {
            var fresh = await SafeBatchCall(ct, TimeSpan.FromSeconds(2), ct2 => _twelveData.GetBatchPricesAsync(missingOther, ct2));
            foreach (var p in fresh)
            {
                await _cache.SetAsync($"price:{p.Symbol}", p, LivePriceTtl, ct);
                result.Add(p);
            }
        }

        return result;
    }

    public async Task<IReadOnlyList<OhlcCandle>> GetHistoricalDataAsync(Asset asset, string interval, int limit, CancellationToken ct)
    {
        var key = $"ohlc:{asset.Pair}:{interval}:{limit}";
        var cached = await _cache.GetAsync<List<OhlcCandle>>(key, ct);
        if (cached is not null) return cached;

        IReadOnlyList<OhlcCandle> fresh = asset.Type switch
        {
            AssetType.Crypto                                       => await _coinGecko.GetHistoricalDataAsync(asset, interval, limit, ct),
            AssetType.Forex or AssetType.Commodity or AssetType.Stock => await _twelveData.GetHistoricalDataAsync(asset, interval, limit, ct),
            _ => Array.Empty<OhlcCandle>()
        };

        if (fresh.Count > 0)
            await _cache.SetAsync(key, fresh.ToList(), HistoryTtl, ct);

        return fresh;
    }


    public async Task<IReadOnlyList<PriceCandle>> GetCandlesAsync(
        string pair, string timeFrame, DateTime fromUtc, DateTime toUtc, CancellationToken ct)
    {
        // Image-only analyses store an arbitrary user-typed pair string.
        // If we can't match it to a tracked Asset, we have no provider to call.
        var asset = await _db.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Pair == pair && a.IsActive, ct);
        if (asset is null)
            return Array.Empty<PriceCandle>();

        // Reuse the existing OHLC path; it already handles caching + provider routing.
        var ohlc = await GetHistoricalDataAsync(asset, timeFrame, CandleFetchLimit, ct);

        // Filter to the requested window and project OhlcCandle → PriceCandle.
        return ohlc
            .Where(c => c.Timestamp >= fromUtc && c.Timestamp <= toUtc)
            .Select(c => new PriceCandle(c.Timestamp, c.Open, c.High, c.Low, c.Close))
            .ToList();
    }
}
