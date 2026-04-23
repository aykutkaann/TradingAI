using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.Analyses.Dtos;
using TradingAI.Application.Features.Analyses.Mapping;
using TradingAI.Domain.Entities;

namespace TradingAI.Application.Features.Analyses.Queries.GetMyAnalyses
{
    public class GetMyAnalysesQueryHandler(IApplicationDbContext db) : IRequestHandler<GetMyAnalysesQuery, PagedResult<AnalysisDto>>
    {


        public async Task<PagedResult<AnalysisDto>> Handle(GetMyAnalysesQuery request, CancellationToken ct)
        {
            var query = db.Analyses.AsNoTracking().Where(a => a.UserId == request.UserId).OrderByDescending(a => a.CreatedAt);

            var total = await query.CountAsync(ct);

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(a => a.Asset)
                .Include(a => a.User)
                .Include(a => a.Likes)
                .Include(a => a.Comments)
                .ToListAsync(ct);

            var dtos = items.Select(a => AnalysisMapper.ToDto(a, request.UserId)).ToList();

            return new PagedResult<AnalysisDto>(dtos, total, request.Page, request.PageSize);

        }


    }
}
