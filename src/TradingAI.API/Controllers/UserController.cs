using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingAI.API.Extensions;
using TradingAI.Application.Auth.DTOs;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.UserFollow.Commands.FollowUser;
using TradingAI.Application.Features.UserFollow.Commands.UnfollowUser;
using TradingAI.Application.Features.UserFollow.DTOs;
using TradingAI.Application.Features.UserFollow.Queries.GetFollowers;
using TradingAI.Application.Features.UserFollow.Queries.GetFollowing;
using TradingAI.Application.Features.Users.Commands.UpdateProfile;
using TradingAI.Application.Features.Users.GetCurrentUser;
using TradingAI.Application.Features.Users.GetPublicProfile;


namespace TradingAI.API.Controllers
{

    [ApiController]
    [Route("api/users")]
    [Authorize]
    [Tags("users")]
    public class UserController(ISender mediator) :ControllerBase
    {
        [HttpPost("{id:guid}/follow")]
        public async Task<IActionResult> Follow(Guid id, CancellationToken ct)
        {
            await mediator.Send(new FollowUserCommand(User.GetUserId(), id), ct);
            return NoContent();
        }

        [HttpDelete("{id:guid}/follow")]
        public async Task<IActionResult> Unfollow(Guid id, CancellationToken ct)
        {
            await mediator.Send(new UnfollowUserCommand(User.GetUserId(), id), ct);
            return NoContent();
        }

        [HttpGet("{id:guid}/followers")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<UserSummaryDto>>> GetFollowers(
            Guid id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            Guid? currentUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
            var result = await mediator.Send(
                new GetFollowersQuery(id, currentUserId, page, pageSize), ct);
            return Ok(result);
        }

        [HttpGet("{id:guid}/following")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<UserSummaryDto>>> GetFollowing(
            Guid id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            Guid? currentUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
            var result = await mediator.Send(
                new GetFollowingQuery(id, currentUserId, page, pageSize), ct);
            return Ok(result);
        }


    }
}
