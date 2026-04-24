using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.Analyses.Dtos;
using TradingAI.Application.Features.Analyses.Mapping;

namespace TradingAI.Application.Features.Analyses.Queries.GetFollowingFeed
{
    public class GetFollowingFeedQueryHandler(IApplicationDbContext db) : IRequestHandler<GetFollowingFeedQuery, PagedResult<AnalysisDto>>
    {
        public async Task<PagedResult<AnalysisDto>> Handle(GetFollowingFeedQuery request, CancellationToken ct)
        {
            var currentUserFollow = await db.UserFollows
                .Where(f => f.FollowerId == request.CurrentUserId)
                .Select(f => f.FollowingId).ToListAsync(ct);

            var query = db.Analyses.Where(a => a.IsPublished && currentUserFollow.Contains(a.UserId)).OrderByDescending(a => a.CreatedAt);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(a => a.Likes)
                .Include(a => a.Comments)
                .Include(a => a.User)
                .ToListAsync(ct);

            var dtos = items.Select(a => AnalysisMapper.ToDto(a, request.CurrentUserId)).ToList();

            return new PagedResult<AnalysisDto>(dtos, totalCount, request.Page, request.PageSize);
        }
    }
}
