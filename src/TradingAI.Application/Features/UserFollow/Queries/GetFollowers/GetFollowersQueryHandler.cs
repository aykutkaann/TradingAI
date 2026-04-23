using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.UserFollow.DTOs;

namespace TradingAI.Application.Features.UserFollow.Queries.GetFollowers
{

    public class GetFollowersQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetFollowersQuery, PagedResult<UserSummaryDto>>
    {
        public async Task<PagedResult<UserSummaryDto>> Handle(GetFollowersQuery request, CancellationToken ct)
        {
            // Target user exists?
            var exists = await db.Users.AnyAsync(u => u.Id == request.UserId, ct);
            if (!exists) throw new NotFoundException("User not found.");

            var query = db.UserFollows
                .AsNoTracking()
                .Where(f => f.FollowingId == request.UserId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => f.Follower);

            var total = await query.CountAsync(ct);

            var users = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new UserSummaryDto(
                    u.Id,
                    u.UserName,
                    u.DisplayName,
                    u.AvatarUrl,
                    u.Bio,
                    u.Followers.Count,
                    u.Following.Count,
                    request.CurrentUserId != null
                        && u.Followers.Any(f => f.FollowerId == request.CurrentUserId)
                ))
                .ToListAsync(ct);

            return new PagedResult<UserSummaryDto>(users, total, request.Page, request.PageSize);
        }
    }
}
