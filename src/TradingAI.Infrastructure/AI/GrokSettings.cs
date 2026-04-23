namespace TradingAI.Infrastructure.Ai;

public class GrokSettings
{
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "grok-2-vision-latest";
    public string BaseUrl { get; set; } = "https://api.x.ai/v1";
    public int MaxTokens { get; set; } = 2048;
}
