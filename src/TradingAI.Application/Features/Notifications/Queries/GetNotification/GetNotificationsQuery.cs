using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.Notifications.DTOs;

namespace TradingAI.Application.Features.Notifications.Queries.GetNotification
{

    public record GetNotificationsQuery(Guid UserId, int Page, int PageSize, bool? UnreadOnly) : IRequest<PagedResult<NotificationDto>>;
}
