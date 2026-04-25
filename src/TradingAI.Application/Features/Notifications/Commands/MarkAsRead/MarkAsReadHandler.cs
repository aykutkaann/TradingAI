using MediatR;
using Microsoft.EntityFrameworkCore;

using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Features.Notifications.Commands.MarkAsRead
{
    public class MarkAsReadHandler(IApplicationDbContext db) : IRequestHandler<MarkAsReadCommand, Unit>
    {
        public async Task<Unit> Handle(MarkAsReadCommand request, CancellationToken ct)
        {
            var notification = await db.Notifications.FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId, ct);

            if (notification == null)
                throw new NotFoundException("Notification not found.");

            if (notification.IsRead) return Unit.Value;
       
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);
            return Unit.Value;





        }
    }
}
