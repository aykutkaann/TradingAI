using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Features.Notifications.Queries.GetUnreadCount
{
    public class GetUnreadCountHandler(IApplicationDbContext db) : IRequestHandler<GetUnreadCountQuery, int>
    {
        public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken ct)
        {
            return await db.Notifications.CountAsync(n => n.UserId == request.UserId && !n.IsRead, ct);
        }

    }
}
