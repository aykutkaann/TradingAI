using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Features.Analyses.Commands.LikeAnalysis
{
    public class LikeAnalysisCommandHandler(IApplicationDbContext db) : IRequestHandler<LikeAnalysisCommand , Unit>
    {
        public async Task<Unit> Handle(LikeAnalysisCommand request, CancellationToken ct)
        {
            var analysis = await db.Analyses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request.AnalysisId, ct) ?? throw new NotFoundException("Analysis not found.");

            var isOwner = analysis.UserId == request.CurrentUserId;

            if (!isOwner && !analysis.IsPublished)
                throw new NotFoundException("Analysis not found.");

            var alreadyLiked = await db.AnalysisLikes
                .AnyAsync(a => a.UserId == request.CurrentUserId && a.AnalysisId == request.AnalysisId, ct);

            if (alreadyLiked) return Unit.Value;

            db.AnalysisLikes.Add(new Domain.Entities.AnalysisLike
            {
                Id = Guid.NewGuid(),
                UserId = request.CurrentUserId,
                AnalysisId = request.AnalysisId,
                CreatedAt = DateTime.UtcNow
            });

            try 
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {

            }
            return Unit.Value;

        }
    }
}
