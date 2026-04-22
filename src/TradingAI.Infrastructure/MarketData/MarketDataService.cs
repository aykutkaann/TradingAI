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

    private static readonly TimeSpan LivePriceTtl = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan HistoryTtl   = TimeSpan.FromMinutes(5);

    public MarketDataService(CoinGeckoClient coinGecko, TwelveDataClient twelveData, ICacheService cache)
    {
        _coinGecko = coinGecko;
        _twelveData = twelveData;
        _cache = cache;
    }

    public async Task<PriceData?> GetLivePriceAsync(Asset asset, CancellationToken ct)
    {
        var key = $"price:{asset.Pair}";
        var cached = await _cache.GetAsync<PriceData>(key, ct);
        if (cached is not null) return cached;

        var fresh = asset.Type switch
        {
            AssetType.Crypto                                       => await _coinGecko.GetLivePriceAsync(asset, ct),
            AssetType.Forex or AssetType.Commodity or AssetType.Stock => await _twelveData.GetLivePriceAsync(asset, ct),
            _ => null
        };

        if (fresh is not null)
            await _cache.SetAsync(key, fresh, LivePriceTtl, ct);

        return fresh;
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
            var fresh = await _coinGecko.GetBatchPricesAsync(missingCrypto, ct);
            foreach (var p in fresh)
            {
                await _cache.SetAsync($"price:{p.Symbol}", p, LivePriceTtl, ct);
                result.Add(p);
            }
        }

        if (missingOther.Count > 0)
        {
            var fresh = await _twelveData.GetBatchPricesAsync(missingOther, ct);
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
}
