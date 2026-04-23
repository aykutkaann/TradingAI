using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Features.Analyses.Dtos;
using TradingAI.Application.Features.Analyses.Mapping;
using TradingAI.Domain.Entities;

namespace TradingAI.Application.Features.Analyses.Queries.GetAnalysisById
{
    public class GetAnalysisByIdQueryHandler(IApplicationDbContext db) : IRequestHandler<GetAnalysisByIdQuery, AnalysisDto>
    {
        public async Task<AnalysisDto> Handle(GetAnalysisByIdQuery request, CancellationToken ct)
        {

            var analysis = await db.Analyses
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Asset)          // null for image uploads — that's fine, EF handles it
                .FirstOrDefaultAsync(a => a.Id == request.AnalysisId, ct);

            if (analysis is null)
                throw new NotFoundException("Analysis not found");

            var isOwner = request.CurrentUserId.HasValue && analysis.UserId == request.CurrentUserId.Value;
            if(!isOwner && !analysis.IsPublished)
                throw new NotFoundException("Analysis  not found.");


            return AnalysisMapper.ToDto(analysis, request.CurrentUserId);
        }



    }
}
