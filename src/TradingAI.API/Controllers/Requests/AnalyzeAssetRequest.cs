namespace TradingAI.API.Controllers.Requests
{

    public record AnalyzeAssetRequest(Guid AssetId, string TimeFrame, string? UserPrompt);

}
