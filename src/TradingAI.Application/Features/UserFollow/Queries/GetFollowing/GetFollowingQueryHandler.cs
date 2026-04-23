using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.UserFollow.DTOs;
using TradingAI.Domain.Entities;

namespace TradingAI.Application.Features.UserFollow.Queries.GetFollowing
{
    public class GetFollowingQueryHandler(IApplicationDbContext db) : IRequestHandler<GetFollowingQuery,PagedResult<UserSummaryDto>>
    {

        public async Task<PagedResult<UserSummaryDto>> Handle(GetFollowingQuery request , CancellationToken ct)
        {
            // Handler — only the Where clause changes
            var query = db.UserFollows
            .AsNoTracking()
            .Where(f => f.FollowerId == request.UserId)       // rows where user X is the Follower
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => f.Following);                    // pull the people they follow

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
