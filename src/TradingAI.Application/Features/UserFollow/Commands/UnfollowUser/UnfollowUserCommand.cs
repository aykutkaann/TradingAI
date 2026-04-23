using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.UserFollow.Commands.UnfollowUser
{

    public record UnfollowUserCommand(Guid FollowerId, Guid FollowingId) : IRequest<Unit>;
}
