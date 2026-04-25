using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Enums;

namespace TradingAI.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }


        public NotificationType Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public Guid? ActorUserId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
