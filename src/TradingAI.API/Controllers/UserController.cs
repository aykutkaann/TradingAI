using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingAI.API.Extensions;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.Stats.DTOs;
using TradingAI.Application.Features.Stats.GetPlatformStats;
using TradingAI.Application.Features.UserFollow.Commands.FollowUser;
using TradingAI.Application.Features.UserFollow.Commands.UnfollowUser;
using TradingAI.Application.Features.UserFollow.DTOs;
using TradingAI.Application.Features.UserFollow.Queries.GetFollowers;
using TradingAI.Application.Features.UserFollow.Queries.GetFollowing;
using TradingAI.Application.Auth.DTOs;
using TradingAI.Application.Features.Users.Commands.UpdateProfile;
using TradingAI.Application.Features.Users.Commands.UploadAvatar;
using TradingAI.Application.Features.Users.DTOs;
using TradingAI.Application.Features.Users.GetPublicProfile;
using TradingAI.Application.Features.Users.GetUserStats;


namespace TradingAI.API.Controllers
{

    [ApiController]
    [Route("api/users")]
    [Authorize]
    [Tags("users")]
    public class UserController(ISender mediator) :ControllerBase
    {
        // PUT /api/users/me — update display name, bio (avatar via separate endpoint)
        [HttpPut("me")]
        public async Task<ActionResult<UserDto>> UpdateMe(
            [FromBody] UpdateMeRequest body, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await mediator.Send(
                new UpdateProfileCommand(userId, body.DisplayName, body.Bio, AvatarUrl: null), ct);
            return Ok(result);
        }

        // POST /api/users/me/avatar — multipart/form-data, field name "File"
        [HttpPost("me/avatar")]
        [RequestSizeLimit(2 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 2 * 1024 * 1024)]
        public async Task<ActionResult<UserDto>> UploadAvatar(
            [FromForm] AvatarUploadRequest body, CancellationToken ct)
        {
            if (body.File is null || body.File.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            await using var stream = body.File.OpenReadStream();
            var result = await mediator.Send(
                new UploadAvatarCommand(
                    UserId: User.GetUserId(),
                    Stream: stream,
                    FileName: body.File.FileName,
                    ContentType: body.File.ContentType ?? "application/octet-stream"),
                ct);
            return Ok(result);
        }

        public record UpdateMeRequest(string? DisplayName, string? Bio);
        public record AvatarUploadRequest(IFormFile? File);

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


        //GET public profile

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<PublicProfileDto>> GetPublicProfile(Guid id, CancellationToken ct)
        {
            Guid? currentUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;

            var result = await mediator.Send(new GetPublicProfileQuery(id, currentUserId), ct);

            return Ok(result);
        }


        //GET user stats
        [HttpGet("{id:guid}/stats")]
        [AllowAnonymous]
        public async Task<ActionResult<UserStatsDto>> GetUserStats(Guid id, CancellationToken ct)
        {
            var result = await mediator.Send(new GetUserStatsQuery(id), ct);

            return Ok(result);
        }

        //GET platform stats
        [HttpGet("stats")]
        [AllowAnonymous]
        public async Task<ActionResult<PlatformStatsDto>> GetPlatformStats(CancellationToken ct)
        {
            var result = await mediator.Send(new GetPlatformStatsQuery(), ct);
            return Ok(result);
        }





    }
}
