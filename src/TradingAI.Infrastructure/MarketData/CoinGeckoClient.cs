using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TradingAI.Application.Common.Models;
using TradingAI.Domain.Entities;

namespace TradingAI.Infrastructure.MarketData;

public class CoinGeckoClient
{
    private readonly HttpClient _http;

    public CoinGeckoClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<PriceData?> GetLivePriceAsync(Asset asset, CancellationToken ct)
    {
        var list = await GetBatchPricesAsync(new[] { asset }, ct);
        return list.FirstOrDefault();
    }

    public async Task<IReadOnlyList<PriceData>> GetBatchPricesAsync(IReadOnlyList<Asset> assets, CancellationToken ct)
    {
        if (assets.Count == 0) return Array.Empty<PriceData>();

        var ids = string.Join(",", assets.Select(a => a.DataSourceId));
        var url = $"/api/v3/coins/markets?vs_currency=usd&ids={ids}";

        var response = await _http.GetFromJsonAsync<List<CoinGeckoMarketItem>>(url, ct);
        if (response is null) return Array.Empty<PriceData>();

        // map back using DataSourceId -> Asset
        var bySourceId = assets.ToDictionary(a => a.DataSourceId, a => a);
        var result = new List<PriceData>();

        foreach (var item in response)
        {
            if (!bySourceId.TryGetValue(item.Id, out var asset)) continue;

            result.Add(new PriceData(
                Symbol: asset.Pair,
                CurrentPrice: item.CurrentPrice,
                Change24h: item.PriceChange24h ?? 0m,
                ChangePercenth24h: item.PriceChangePercentage24h ?? 0m,
                High24h: item.High24h ?? 0m,
                Low24h: item.Low24h ?? 0m,
                Volume24h: item.TotalVolume ?? 0m,
                UpdatedAt: item.LastUpdated ?? DateTime.UtcNow
            ));
        }

        return result;
    }

    public async Task<IReadOnlyList<OhlcCandle>> GetHistoricalDataAsync(Asset asset, string interval, int limit, CancellationToken ct)
    {
        // CoinGecko OHLC endpoint accepts days (1,7,14,30,90,180,365,max). Map "limit" to days.
        var days = interval switch
        {
            "5m" or "15m" or "1H" => 1,
            "4H" => 7,
            "1D" => Math.Min(Math.Max(limit, 1), 365),
            "1W" or "1M" => 365,
            _ => 30
        };

        var url = $"/api/v3/coins/{asset.DataSourceId}/ohlc?vs_currency=usd&days={days}";
        var raw = await _http.GetFromJsonAsync<List<List<decimal>>>(url, ct);
        if (raw is null) return Array.Empty<OhlcCandle>();

        var candles = raw.Select(c => new OhlcCandle(
            Timestamp: DateTimeOffset.FromUnixTimeMilliseconds((long)c[0]).UtcDateTime,
            Open:  c[1],
            High:  c[2],
            Low:   c[3],
            Close: c[4],
            Volume: 0m // CoinGecko free OHLC endpoint doesn't return volume
        )).ToList();

        return candles.TakeLast(limit).ToList();
    }

    // --- Internal response DTOs ---

    private sealed class CoinGeckoMarketItem
    {
        [JsonPropertyName("id")]                           public string Id { get; set; } = null!;
        [JsonPropertyName("current_price")]                public decimal CurrentPrice { get; set; }
        [JsonPropertyName("price_change_24h")]             public decimal? PriceChange24h { get; set; }
        [JsonPropertyName("price_change_percentage_24h")]  public decimal? PriceChangePercentage24h { get; set; }
        [JsonPropertyName("high_24h")]                     public decimal? High24h { get; set; }
        [JsonPropertyName("low_24h")]                      public decimal? Low24h { get; set; }
        [JsonPropertyName("total_volume")]                 public decimal? TotalVolume { get; set; }
        [JsonPropertyName("last_updated")]                 public DateTime? LastUpdated { get; set; }
    }
}
