using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Features.Comments.Commands.DeleteComment
{
    public class DeleteCommentCommandHandler(IApplicationDbContext db): IRequestHandler<DeleteCommentCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteCommentCommand request, CancellationToken ct)
        {
            var comment = await db.AnalysisComments
                .Include(c => c.Analysis)
                .FirstOrDefaultAsync(c => c.Id == request.CommentId, ct) ?? throw new NotFoundException("Comment not found.");

            //Who can delete
            //1- comment author
            //2- owner of that ananlysis (like modarate)
            var canDelete = comment.UserId == request.CurrentUserId || comment.Analysis.UserId == request.CurrentUserId;

            if (!canDelete)
                throw new NotFoundException("Comment not found.");

            db.AnalysisComments.Remove(comment);

            await db.SaveChangesAsync(ct);

            return Unit.Value;
            
        }
    }
}
