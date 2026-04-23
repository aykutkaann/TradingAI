using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Features.Comments.DTOs;
using TradingAI.Domain.Entities;

namespace TradingAI.Application.Features.Comments.Commands.CreateComment
{
    public class CreateCommentCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateCommentCommand, CommentDto>
    {
        public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken ct)
        {
            var analysis = await db.Analyses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request.AnalysisId, ct)
                ?? throw new NotFoundException("Analysis not found.");

            var isOwner = analysis.UserId == request.CurrentUserId;
            if(!isOwner && !analysis.IsPublished)
                throw new NotFoundException("Analysis not found.");

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.CurrentUserId, ct)
                ?? throw new UnauthorizedException("Invalid Credential");

            var comment = new AnalysisComment
            {
                Id = Guid.NewGuid(),
                AnalysisId = request.AnalysisId,
                UserId = request.CurrentUserId,
                Content = request.Content.Trim(),
                CreatedAt = DateTime.UtcNow

            };

            db.AnalysisComments.Add(comment);
            await db.SaveChangesAsync(ct);

            return new CommentDto(
                Id: comment.Id,
                AnalysisId: comment.AnalysisId,
                UserId: comment.UserId,
                UserDisplayName: user.DisplayName ?? user.UserName,
                UserAvatarUrl: user.AvatarUrl,
                Content: comment.Content,
                CreatedAt: comment.CreatedAt
                );

        }
    }
}
