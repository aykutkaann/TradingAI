using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Features.Analyses.Commands.UnlikeAnalysis
{
    public class UnlikeAnalysisCommandHandler(IApplicationDbContext db) : IRequestHandler<UnlikeAnalysisCommand, Unit>
    {

        public async Task<Unit> Handle(UnlikeAnalysisCommand request , CancellationToken ct)
        {

            var like = await db.AnalysisLikes
                .FirstOrDefaultAsync(a => a.UserId == request.CurrentUserId && a.AnalysisId == request.AnalysisId, ct);


            if (like is null) return Unit.Value;

            db.AnalysisLikes.Remove(like);

            await db.SaveChangesAsync(ct);

            return Unit.Value;

        }
    }
}
