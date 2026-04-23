using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.UserFollow.Commands.FollowUser
{

    public record FollowUserCommand(Guid FollowerId, Guid FollowingId) : IRequest<Unit>;
}
