namespace TradingAI.API.Controllers.Requests
{

    public class AnalyzeImageRequest
    {
        public IFormFile? File { get; set; }
        public string AssetPair { get; set; } = "";
        public string TimeFrame { get; set; } = "";
        public string? UserPrompt { get; set; }
    }
}
