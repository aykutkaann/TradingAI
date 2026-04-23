using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Schema;

namespace TradingAI.Application.Features.Comments.DTOs
{

    public record CommentDto(Guid Id, Guid AnalysisId, Guid UserId,
        string UserDisplayName, string? UserAvatarUrl, string Content, DateTime CreatedAt);
}
