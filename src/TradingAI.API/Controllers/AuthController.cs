using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingAI.API.Auth.DTOs;
using TradingAI.Application.Auth.Commands.ChangePassword;
using TradingAI.Application.Auth.Commands.LoginUser;
using TradingAI.Application.Auth.Commands.RefreshToken;
using TradingAI.Application.Auth.Commands.RegisterUser;
using TradingAI.Application.Auth.Commands.RevokeToken;
using TradingAI.API.Extensions;
using TradingAI.Application.Auth.DTOs;

namespace TradingAI.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterUserDto request)
    {
        var command = new RegisterUserCommand(request.Email, request.UserName, request.Password, GetIpAddress());
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginDto request)
    {
        var command = new LoginCommand(request.Email, request.Password, GetIpAddress());
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenDto request)
    {
        var command = new RefreshTokenCommand(request.Token, GetIpAddress());
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> Revoke(RevokeTokenDto request)
    {
        var command = new RevokeTokenCommand(request.Token, GetIpAddress());
        await mediator.Send(command);
        return NoContent();
    }

    // POST /api/auth/change-password
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await mediator.Send(
            new ChangePasswordCommand(User.GetUserId(), request.CurrentPassword, request.NewPassword),
            ct);
        return NoContent();
    }

    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"]!;

        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";
    }
}
