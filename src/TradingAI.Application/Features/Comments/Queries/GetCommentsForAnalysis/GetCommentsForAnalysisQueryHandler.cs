using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.Comments.DTOs;

namespace TradingAI.Application.Features.Comments.Queries.GetCommentsForAnalysis
{
    public class GetCommentsForAnalysisQueryHandler(IApplicationDbContext db)
        : IRequestHandler<GetCommentsForAnalysisQuery, PagedResult<CommentDto>>
    {
        public async Task<PagedResult<CommentDto>> Handle(GetCommentsForAnalysisQuery request, CancellationToken ct)
        {
            var analysis = await db.Analyses.AsNoTracking().FirstOrDefaultAsync(a => a.Id == request.AnalysisId, ct)
                ?? throw new NotFoundException("Analysis not found.");

            var isOwner = request.CurrentUserId == analysis.UserId;

            if(!isOwner && !analysis.IsPublished)
                throw new NotFoundException("Analysis not found.");

            var query = db.AnalysisComments.AsNoTracking().Where(c => c.AnalysisId == request.AnalysisId).OrderBy(c => c.CreatedAt);

            var total = await query.CountAsync(ct);

            var comment = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(c => c.User)
                .Select(c => new CommentDto(
                    c.Id,
                    c.AnalysisId,
                    c.UserId,
                    c.User.DisplayName ?? c.User.UserName,
                    c.User.AvatarUrl,
                    c.Content,
                    c.CreatedAt
                    )).ToListAsync(ct);

            return new PagedResult<CommentDto>(comment, total, request.Page, request.PageSize);

        }
    }
}
