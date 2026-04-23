using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.UserFollow.DTOs;

namespace TradingAI.Application.Features.UserFollow.Queries.GetFollowers
{

    public record GetFollowersQuery(
        Guid UserId,                 
        Guid? CurrentUserId,         
        int Page = 1,
        int PageSize = 50
    ) : IRequest<PagedResult<UserSummaryDto>>;
}
