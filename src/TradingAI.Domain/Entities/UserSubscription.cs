using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Enums;

namespace TradingAI.Domain.Entities
{
    public class UserSubscription
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public Guid PlanId { get; set; }
        public SubscriptionPlan Plan { get; set; } = null!;


        public SubscriptionTier Tier { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string? PaymentReference { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public User User { get; set; }
    }
}
