using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Features.Comments.DTOs;

namespace TradingAI.Application.Features.Comments.Commands.DeleteComment
{

    public record DeleteCommentCommand(Guid CommentId, Guid CurrentUserId) : IRequest<Unit>;
}
