using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Features.Analyses.Commands.DeleteAnalysis
{
    public class DeleteAnalysisCommandHandler(IApplicationDbContext db, IFileStorage fileStorage): IRequestHandler<DeleteAnalysisCommand, Unit>
    {

        public async Task<Unit> Handle(DeleteAnalysisCommand request, CancellationToken ct)
        {
            var analysis = await db.Analyses
                .FirstOrDefaultAsync(a => a.Id == request.AnalysisId, ct) ?? throw new NotFiniteNumberException("Analyses not foun.");

            if (analysis.UserId != request.CurrentUserId)
                throw new NotFoundException("Analysis not found.");

            var imageUrlToDelete = analysis.ImageUrl;

            db.Analyses.Remove(analysis);

            await db.SaveChangesAsync(ct);

            if (!string.IsNullOrEmpty(imageUrlToDelete))
            {
                await fileStorage.DeleteAsync(imageUrlToDelete, ct);
            }

            return Unit.Value;
                            
        }
    }
}
