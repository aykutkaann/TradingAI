using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.Notifications.Commands.MarkAllAsRead
{

    public record MarkAllAsReadCommand(Guid UserId) : IRequest<int>;
}
