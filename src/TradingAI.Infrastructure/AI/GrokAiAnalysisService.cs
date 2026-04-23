using System.ClientModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.Models;

namespace TradingAI.Infrastructure.Ai;

/// <summary>
/// AI analysis powered by xAI's Grok models via the OpenAI-compatible endpoint.
/// </summary>
public class GrokAiAnalysisService : IAiAnalysisService
{
    private readonly ChatClient _chat;
    private readonly GrokSettings _settings;
    private readonly ILogger<GrokAiAnalysisService> _logger;


    private const string SystemPrompt = """
        You are an expert technical analyst reviewing trading charts and price data.

        TRADE LEVEL RULES — you MUST follow these:
        - If trendDirection is "Bullish" (long bias):
            * stopLoss MUST be LOWER than suggestedEntry
            * takeProfit1 and takeProfit2 MUST be HIGHER than suggestedEntry
            * takeProfit2 should be HIGHER than takeProfit1
        - If trendDirection is "Bearish" (short bias):
            * stopLoss MUST be HIGHER than suggestedEntry
            * takeProfit1 and takeProfit2 MUST be LOWER than suggestedEntry
            * takeProfit2 should be LOWER than takeProfit1
        - If trendDirection is "Neutral": return null for suggestedEntry, stopLoss, takeProfit1, takeProfit2, riskRewardRatio
        - riskRewardRatio = |takeProfit1 - suggestedEntry| / |suggestedEntry - stopLoss|
          It must be POSITIVE. Do not return a negative ratio.

        Double-check the sign of every numeric level against these rules BEFORE returning your JSON.

        Respond ONLY with a JSON object matching this exact shape (no markdown fences, no commentary):
        {
          "trendDirection": "Bullish" | "Bearish" | "Neutral",
          "detectedPatterns": string[],
          "keyLevels": { "support": number[], "resistance": number[] },
          "currentPrice": number | null,
          "suggestedEntry": number | null,
          "stopLoss": number | null,
          "takeProfit1": number | null,
          "takeProfit2": number | null,
          "riskRewardRatio": number | null,
          "analysis": string,
          "summary": string
        }
        - "analysis" is a detailed markdown report (200–400 words).
        - "summary" is 2–3 sentences for display cards.
        - If a numeric value cannot be inferred with confidence, return null. Do not guess.
        - This is educational analysis only, not financial advice.
        """;

    public GrokAiAnalysisService(
        IOptions<GrokSettings> settings,
        ILogger<GrokAiAnalysisService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var client = new OpenAIClient(
            new ApiKeyCredential(_settings.ApiKey),
            new OpenAIClientOptions { Endpoint = new Uri(_settings.BaseUrl) });

        _chat = client.GetChatClient(_settings.Model);
    }

    public async Task<AiAnalysisResult> AnalyzeChartImageAsync(
        Stream imageStream,
        string imageMediaType,
        string assetPair,
        string timeframe,
        string? userPrompt,
        CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await imageStream.CopyToAsync(ms, ct);
        var imageBytes = BinaryData.FromBytes(ms.ToArray());

        var userText = $"Analyze this {assetPair} chart on the {timeframe} timeframe.";
        if (!string.IsNullOrWhiteSpace(userPrompt))
            userText += $"\n\nAdditional context from the user: {userPrompt}";

        var userMessage = new UserChatMessage(
            ChatMessageContentPart.CreateImagePart(imageBytes, imageMediaType),
            ChatMessageContentPart.CreateTextPart(userText));

        return await SendAsync(new ChatMessage[]
        {
            new SystemChatMessage(SystemPrompt),
            userMessage
        }, ct);
    }

    public async Task<AiAnalysisResult> AnalyzeAssetAsync(
        string assetPair,
        string timeframe,
        decimal currentPrice,
        IReadOnlyList<OhlcCandle> candles,
        string? userPrompt,
        CancellationToken ct)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Analyze {assetPair} on the {timeframe} timeframe.");
        sb.AppendLine($"Current price: {currentPrice}");
        sb.AppendLine("Recent OHLC candles (oldest first):");
        foreach (var c in candles)
        {
            sb.AppendLine($"{c.Timestamp:yyyy-MM-dd HH:mm}  O:{c.Open}  H:{c.High}  L:{c.Low}  C:{c.Close}  V:{c.Volume}");
        }

        if (!string.IsNullOrWhiteSpace(userPrompt))
            sb.AppendLine($"\nUser note: {userPrompt}");

        return await SendAsync(new ChatMessage[]
        {
            new SystemChatMessage(SystemPrompt),
            new UserChatMessage(sb.ToString())
        }, ct);
    }

    private async Task<AiAnalysisResult> SendAsync(IEnumerable<ChatMessage> messages, CancellationToken ct)
    {
        var options = new ChatCompletionOptions
        {
            Temperature = 0.3f,
            MaxOutputTokenCount = _settings.MaxTokens
        };

        var response = await _chat.CompleteChatAsync(messages, options, ct);

        var text = response.Value.Content.Count > 0
            ? response.Value.Content[0].Text ?? string.Empty
            : string.Empty;

        _logger.LogInformation("Grok raw response length: {Length}", text.Length);

        var json = ExtractJson(text);
        if (json is null)
            throw new InvalidOperationException("Grok did not return valid JSON.");

        var parsed = JsonSerializer.Deserialize<GrokResponse>(json, JsonOpts)
                     ?? throw new InvalidOperationException("Failed to deserialize Grok response.");

        return new AiAnalysisResult(
            TrendDirection:    parsed.TrendDirection ?? "Neutral",
            DetectedPatterns:  (IReadOnlyList<string>?)parsed.DetectedPatterns ?? Array.Empty<string>(),
            KeyLevels:         new KeyLevels(
                                    (IReadOnlyList<decimal>?)parsed.KeyLevels?.Support ?? Array.Empty<decimal>(),
                                    (IReadOnlyList<decimal>?)parsed.KeyLevels?.Resistance ?? Array.Empty<decimal>()),
            SuggestedEntry:    parsed.SuggestedEntry,
            StopLoss:          parsed.StopLoss,
            TakeProfit1:       parsed.TakeProfit1,
            TakeProfit2:       parsed.TakeProfit2,
            RiskRewardRatio:   parsed.RiskRewardRatio,
            Analysis:          parsed.Analysis ?? string.Empty,
            Summary:           parsed.Summary ?? string.Empty
        );
    }

    // Grok sometimes wraps JSON in ```json ... ``` fences. Strip them.
    private static string? ExtractJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var fenced = Regex.Match(text, @"```(?:json)?\s*([\s\S]*?)\s*```", RegexOptions.IgnoreCase);
        if (fenced.Success) return fenced.Groups[1].Value.Trim();

        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start >= 0 && end > start) return text.Substring(start, end - start + 1);

        return null;
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private sealed class GrokResponse
    {
        [JsonPropertyName("trendDirection")]   public string? TrendDirection { get; set; }
        [JsonPropertyName("detectedPatterns")] public List<string>? DetectedPatterns { get; set; }
        [JsonPropertyName("keyLevels")]        public GrokKeyLevels? KeyLevels { get; set; }
        [JsonPropertyName("suggestedEntry")]   public decimal? SuggestedEntry { get; set; }
        [JsonPropertyName("stopLoss")]         public decimal? StopLoss { get; set; }
        [JsonPropertyName("takeProfit1")]      public decimal? TakeProfit1 { get; set; }
        [JsonPropertyName("takeProfit2")]      public decimal? TakeProfit2 { get; set; }
        [JsonPropertyName("riskRewardRatio")]  public decimal? RiskRewardRatio { get; set; }
        [JsonPropertyName("analysis")]         public string? Analysis { get; set; }
        [JsonPropertyName("summary")]          public string? Summary { get; set; }
    }

    private sealed class GrokKeyLevels
    {
        [JsonPropertyName("support")]    public List<decimal>? Support { get; set; }
        [JsonPropertyName("resistance")] public List<decimal>? Resistance { get; set; }
    }
}
