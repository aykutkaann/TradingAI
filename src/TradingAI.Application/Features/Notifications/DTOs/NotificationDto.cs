using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Enums;

namespace TradingAI.Application.Features.Notifications.DTOs
{

    public record NotificationDto(Guid Id, NotificationType Type, string Title, string Message, Guid? RelatedEntityId, Guid? ActorUserId,
        bool IsRead, DateTime CreatedAt, DateTime? ReadAt);
}
