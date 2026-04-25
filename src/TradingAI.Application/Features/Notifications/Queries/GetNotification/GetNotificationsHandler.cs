using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.Notifications.DTOs;

namespace TradingAI.Application.Features.Notifications.Queries.GetNotification
{
    public class GetNotificationsHandler(IApplicationDbContext db):IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
    {
        public async Task<PagedResult<NotificationDto>> Handle(GetNotificationsQuery request ,CancellationToken ct)
        {
            var query =  db.Notifications.AsNoTracking().Where(n => n.UserId == request.UserId);

            if(request.UnreadOnly == true)
            {
                query = query.Where(n => !n.IsRead);
            }

            query = query.OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(n => new NotificationDto(
                    n.Id,
                    n.Type,
                    n.Title,
                    n.Message,
                    n.RelatedEntityId,
                    n.ActorUserId,
                    n.IsRead,
                    n.CreatedAt,
                    n.ReadAt
                    )).ToListAsync(ct);

            return new PagedResult<NotificationDto>(items, totalCount, request.Page, request.PageSize);

        }
    }
}
