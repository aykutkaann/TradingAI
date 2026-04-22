using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingAI.API.Extensions;
using TradingAI.Application.Auth.DTOs;
using TradingAI.Application.Users.Commands.UpdateProfile;
using TradingAI.Application.Users.GetCurrentUser;
using TradingAI.Application.Users.GetPublicProfile;

namespace TradingAI.API.Controllers
{

    [ApiController]
    [Route("api/users")]
    public class UserController(IMediator mediator) :ControllerBase
    {


        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetMe()
        {
            var userId = User.GetUserId();

            return Ok(await mediator.Send(new GetCurrentUserQuery(userId)));
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> UpdateMe(UpdateProfileCommand command)
        {
            var userId = User.GetUserId();
            return Ok(await mediator.Send(command));
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<PublicProfileDto>> GetPublicProfile(string username)
        {
            return Ok(await mediator.Send(new GetPublicProfileQuery(username)));
        }
    }
}
