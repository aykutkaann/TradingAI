using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.Notifications.Commands.MarkAsRead
{

    public record MarkAsReadCommand(Guid UserId, Guid NotificationId) : IRequest<Unit>;
}
