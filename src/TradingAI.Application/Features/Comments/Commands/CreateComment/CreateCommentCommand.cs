using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Features.Comments.DTOs;

namespace TradingAI.Application.Features.Comments.Commands.CreateComment
{
    public record CreateCommentCommand(Guid AnalysisId, Guid CurrentUserId, string Content) : IRequest<CommentDto>;
    
    
}
