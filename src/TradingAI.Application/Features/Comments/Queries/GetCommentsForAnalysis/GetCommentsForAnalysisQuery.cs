using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.Comments.DTOs;

namespace TradingAI.Application.Features.Comments.Queries.GetCommentsForAnalysis
{

    public record GetCommentsForAnalysisQuery(
        Guid AnalysisId,
        Guid? CurrentUserId,
        int Page = 1,
        int PageSize = 50
    ) : IRequest<PagedResult<CommentDto>>;
}
