using TradingAI.Domain.Enums;

namespace TradingAI.API.Controllers.Requests
{

    public record UpgradeSubscriptionRequest(SubscriptionTier NewTier);
}
