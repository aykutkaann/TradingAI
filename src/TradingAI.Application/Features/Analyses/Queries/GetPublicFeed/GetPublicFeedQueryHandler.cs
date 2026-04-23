using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.Analyses.Dtos;
using TradingAI.Application.Features.Analyses.Mapping;
using TradingAI.Domain.Entities;

namespace TradingAI.Application.Features.Analyses.Queries.GetPublicFeed
{
    public class GetPublicFeedQueryHandler(IApplicationDbContext db) :IRequestHandler<GetPublicFeedQuery, PagedResult<AnalysisDto>>
    {

        public async Task<PagedResult<AnalysisDto>> Handle(GetPublicFeedQuery request, CancellationToken ct)
        {
            var baseQuery = db.Analyses
                .AsNoTracking()
                .Where(a => a.IsPublished)
                 .OrderByDescending(a =>a.PublishedAt ?? a.CreatedAt);


            var total = await baseQuery.CountAsync(ct);


            var items = await baseQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(a => a.User)
            .Include(a => a.Asset)
            .Include(a => a.Likes)
            .Include(a => a.Comments)
            .ToListAsync(ct);

            var dtos = items.Select(a => AnalysisMapper.ToDto(a, request.CurrentUserId)).ToList();

            return new PagedResult<AnalysisDto>(dtos, total, request.Page, request.PageSize);
        }
    }
}
