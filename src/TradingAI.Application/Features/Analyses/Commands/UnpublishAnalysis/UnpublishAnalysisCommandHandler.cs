using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Features.Analyses.Commands.UnpublishAnalysis
{
    public class UnpublishAnalysisCommandHandler(IApplicationDbContext db) : IRequestHandler<UnpublishAnalysisCommand, Unit>
    {
        public async Task<Unit> Handle(UnpublishAnalysisCommand request, CancellationToken ct)
        {
            var analysis = await db.Analyses.FirstOrDefaultAsync(a => a.Id == request.AnalysisId, ct)
                ?? throw new NotFoundException("Analysis not found.");

            if(analysis.UserId != request.CurrentUserId)
                throw new NotFoundException("Analysis not found.");

            if (!analysis.IsPublished)
                return Unit.Value;

            analysis.IsPublished = false;

            await db.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
