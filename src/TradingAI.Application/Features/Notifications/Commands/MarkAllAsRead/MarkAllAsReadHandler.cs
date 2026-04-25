using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Features.Notifications.Commands.MarkAllAsRead
{
    public class MarkAllAsReadHandler(IApplicationDbContext db) : IRequestHandler<MarkAllAsReadCommand, int>
    {
        public async Task<int> Handle(MarkAllAsReadCommand request, CancellationToken ct)
        {
            var count = await db.Notifications.Where(n => n.UserId == request.UserId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, DateTime.UtcNow), ct);

            return count;
        }
    }
}
