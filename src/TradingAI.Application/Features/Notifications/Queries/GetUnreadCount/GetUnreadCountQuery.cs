using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.Notifications.Queries.GetUnreadCount
{

    public record GetUnreadCountQuery(Guid UserId) : IRequest<int>;
}
