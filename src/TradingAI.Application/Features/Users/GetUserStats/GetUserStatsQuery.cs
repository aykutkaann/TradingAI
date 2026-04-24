using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Features.Users.DTOs;

namespace TradingAI.Application.Features.Users.GetUserStats
{

    public record GetUserStatsQuery(Guid UserId) : IRequest<UserStatsDto>;
}
