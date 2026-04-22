using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using TradingAI.Application.Common.Models;
using TradingAI.Domain.Entities;

namespace TradingAI.Infrastructure.MarketData;

public class TwelveDataClient
{
    private readonly HttpClient _http;
    private readonly TwelveDataSettings _settings;

    public TwelveDataClient(HttpClient http, IOptions<TwelveDataSettings> settings)
    {
        _http = http;
        _settings = settings.Value;
        if (_http.BaseAddress is null)
            _http.BaseAddress = new Uri(_settings.BaseUrl);
    }

    public async Task<PriceData?> GetLivePriceAsync(Asset asset, CancellationToken ct)
    {
        var url = $"/quote?symbol={Uri.EscapeDataString(asset.Pair)}&apikey={_settings.ApiKey}";
        var quote = await _http.GetFromJsonAsync<TwelveDataQuote>(url, ct);
        if (quote is null || string.IsNullOrEmpty(quote.Close)) return null;

        var price  = ParseDecimal(quote.Close);
        var change = ParseDecimal(quote.Change);
        var pct    = ParseDecimal(quote.PercentChange);
        var high   = ParseDecimal(quote.High);
        var low    = ParseDecimal(quote.Low);
        var vol    = ParseDecimal(quote.Volume);

        return new PriceData(
            Symbol: asset.Pair,
            CurrentPrice: price,
            Change24h: change,
            ChangePercenth24h: pct,
            High24h: high,
            Low24h: low,
            Volume24h: vol,
            UpdatedAt: DateTime.UtcNow
        );
    }

    public async Task<IReadOnlyList<PriceData>> GetBatchPricesAsync(IReadOnlyList<Asset> assets, CancellationToken ct)
    {
        // Twelve Data supports comma-separated symbols on /quote — returns a dictionary keyed by symbol
        if (assets.Count == 0) return Array.Empty<PriceData>();
        if (assets.Count == 1)
        {
            var single = await GetLivePriceAsync(assets[0], ct);
            return single is null ? Array.Empty<PriceData>() : new[] { single };
        }

        var symbols = string.Join(",", assets.Select(a => a.Pair));
        var url = $"/quote?symbol={Uri.EscapeDataString(symbols)}&apikey={_settings.ApiKey}";

        var dict = await _http.GetFromJsonAsync<Dictionary<string, TwelveDataQuote>>(url, ct);
        if (dict is null) return Array.Empty<PriceData>();

        var result = new List<PriceData>();
        foreach (var asset in assets)
        {
            if (!dict.TryGetValue(asset.Pair, out var q) || string.IsNullOrEmpty(q.Close)) continue;

            result.Add(new PriceData(
                Symbol: asset.Pair,
                CurrentPrice: ParseDecimal(q.Close),
                Change24h: ParseDecimal(q.Change),
                ChangePercenth24h: ParseDecimal(q.PercentChange),
                High24h: ParseDecimal(q.High),
                Low24h: ParseDecimal(q.Low),
                Volume24h: ParseDecimal(q.Volume),
                UpdatedAt: DateTime.UtcNow
            ));
        }

        return result;
    }

    public async Task<IReadOnlyList<OhlcCandle>> GetHistoricalDataAsync(Asset asset, string interval, int limit, CancellationToken ct)
    {
        var tdInterval = MapInterval(interval);
        var url = $"/time_series?symbol={Uri.EscapeDataString(asset.Pair)}&interval={tdInterval}&outputsize={limit}&apikey={_settings.ApiKey}";

        var data = await _http.GetFromJsonAsync<TwelveDataTimeSeries>(url, ct);
        if (data?.Values is null) return Array.Empty<OhlcCandle>();

        return data.Values
            .Select(v => new OhlcCandle(
                Timestamp: DateTime.Parse(v.Datetime, CultureInfo.InvariantCulture),
                Open:  ParseDecimal(v.Open),
                High:  ParseDecimal(v.High),
                Low:   ParseDecimal(v.Low),
                Close: ParseDecimal(v.Close),
                Volume: ParseDecimal(v.Volume ?? "0")
            ))
            .Reverse() // TwelveData returns newest-first; we want oldest-first
            .ToList();
    }

    private static decimal ParseDecimal(string? s)
        => decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0m;

    private static string MapInterval(string interval) => interval switch
    {
        "5m"  => "5min",
        "15m" => "15min",
        "1H"  => "1h",
        "4H"  => "4h",
        "1D"  => "1day",
        "1W"  => "1week",
        "1M"  => "1month",
        _     => "1day"
    };

    // --- Internal DTOs ---

    private sealed class TwelveDataQuote
    {
        [JsonPropertyName("symbol")]          public string? Symbol { get; set; }
        [JsonPropertyName("close")]           public string? Close { get; set; }
        [JsonPropertyName("change")]          public string? Change { get; set; }
        [JsonPropertyName("percent_change")]  public string? PercentChange { get; set; }
        [JsonPropertyName("high")]            public string? High { get; set; }
        [JsonPropertyName("low")]             public string? Low { get; set; }
        [JsonPropertyName("volume")]          public string? Volume { get; set; }
    }

    private sealed class TwelveDataTimeSeries
    {
        [JsonPropertyName("values")] public List<TwelveDataCandle>? Values { get; set; }
    }

    private sealed class TwelveDataCandle
    {
        [JsonPropertyName("datetime")] public string Datetime { get; set; } = null!;
        [JsonPropertyName("open")]     public string Open { get; set; } = null!;
        [JsonPropertyName("high")]     public string High { get; set; } = null!;
        [JsonPropertyName("low")]      public string Low { get; set; } = null!;
        [JsonPropertyName("close")]    public string Close { get; set; } = null!;
        [JsonPropertyName("volume")]   public string? Volume { get; set; }
    }
}
