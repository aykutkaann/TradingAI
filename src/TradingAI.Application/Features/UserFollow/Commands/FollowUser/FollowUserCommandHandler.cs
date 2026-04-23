using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Features.UserFollow.Commands.FollowUser
{
    public class FollowUserCommandHandler(IApplicationDbContext db) : IRequestHandler<FollowUserCommand,Unit>
    {
        public async Task<Unit> Handle(FollowUserCommand request, CancellationToken ct)
        {
            if (request.FollowerId == request.FollowingId)
                throw new ValidationException();

            var targetUserExist = await db.Users.AnyAsync(a => a.Id == request.FollowingId, ct);
                
            if(!targetUserExist)
                 throw new NotFoundException("User not found.");


            var alreadyFollowing = await db.UserFollows.AnyAsync(f => f.FollowerId == request.FollowerId
                && f.FollowingId == request.FollowingId, ct);

            if (alreadyFollowing) return Unit.Value;

            db.UserFollows.Add(new Domain.Entities.UserFollow
            {
                Id = Guid.NewGuid(),
                FollowerId = request.FollowerId,
                FollowingId = request.FollowingId,
                CreatedAt = DateTime.UtcNow
            });

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException) { }

            return Unit.Value;
        }
    }
}
