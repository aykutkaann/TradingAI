using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Features.UserFollow.Commands.UnfollowUser
{
    public class UnfollowUserCommandHandler(IApplicationDbContext db) : IRequestHandler<UnfollowUserCommand, Unit>
    {
        public async Task<Unit> Handle(UnfollowUserCommand request, CancellationToken ct)
        {
            var follow = await db.UserFollows
                .FirstOrDefaultAsync(f => f.FollowerId == request.FollowerId && f.FollowingId == request.FollowingId, ct);
            if (follow is null) return Unit.Value;

            db.UserFollows.Remove(follow);

            await db.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}
