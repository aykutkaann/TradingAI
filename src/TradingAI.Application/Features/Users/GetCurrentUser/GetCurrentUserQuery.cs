using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Auth.DTOs;

namespace TradingAI.Application.Features.Users.GetCurrentUser
{

    public record GetCurrentUserQuery(Guid UserId) : IRequest<UserDto>;
}
