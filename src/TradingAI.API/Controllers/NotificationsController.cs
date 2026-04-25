using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.Notifications.Commands.MarkAllAsRead;
using TradingAI.Application.Features.Notifications.Commands.MarkAsRead;
using TradingAI.Application.Features.Notifications.DTOs;
using TradingAI.Application.Features.Notifications.Queries.GetNotification;
using TradingAI.Application.Features.Notifications.Queries.GetUnreadCount;
using TradingAI.Domain.Entities;

namespace TradingAI.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/notifications")]
    public class NotificationsController(IMediator mediator,ICurrentUserService currentUser) :ControllerBase
    {

        [HttpGet]
        public Task<PagedResult<NotificationDto>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? unreadOnly = null,
            CancellationToken ct = default)
            => mediator.Send(new GetNotificationsQuery(currentUser.RequireUserId(), page, pageSize, unreadOnly), ct);

        [HttpGet("unread-count")]
        public Task<int> UnreadCount(CancellationToken ct)
            => mediator.Send(new GetUnreadCountQuery(currentUser.RequireUserId()),ct);

        [HttpPost("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
        {
            await mediator.Send(new MarkAsReadCommand(currentUser.RequireUserId(), id), ct);

            return NoContent();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> ReadAll(CancellationToken ct)
        {
            var count = await mediator.Send(new MarkAllAsReadCommand(currentUser.RequireUserId()) ,ct);

            return Ok(new { markedCount = count });
        }


    }
}
