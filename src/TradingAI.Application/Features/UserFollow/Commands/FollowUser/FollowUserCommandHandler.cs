using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Features.UserFollow.Commands.FollowUser
{
    public class FollowUserCommandHandler(IApplicationDbContext db, INotificationService notifications) : IRequestHandler<FollowUserCommand, Unit>
    {
        public async Task<Unit> Handle(FollowUserCommand request, CancellationToken ct)
        {
            if (request.FollowerId == request.FollowingId)
                throw new ValidationException();

            var targetUserExist = await db.Users.AnyAsync(a => a.Id == request.FollowingId, ct);

            if (!targetUserExist)
                throw new NotFoundException("User not found.");


            var alreadyFollowing = await db.UserFollows.AnyAsync(f => f.FollowerId == request.FollowerId
                && f.FollowingId == request.FollowingId, ct);

            if (alreadyFollowing) return Unit.Value;

            // Load follower for the notification message
            var follower = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.FollowerId, ct)
                ?? throw new UnauthorizedException("Invalid credentials.");

            db.UserFollows.Add(new Domain.Entities.UserFollow
            {
                Id = Guid.NewGuid(),
                FollowerId = request.FollowerId,
                FollowingId = request.FollowingId,
                CreatedAt = DateTime.UtcNow
            });

            await notifications.NotifyNewFollowerAsync(
                recipientUserId: request.FollowingId,
                followerUserId: request.FollowerId,
                followerUserName: follower.DisplayName ?? follower.UserName,
                ct);

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException) { }

            return Unit.Value;
        }
    }
}
