using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Auth.DTOs;

namespace TradingAI.Application.Features.Users.Commands.UpdateProfile
{

    public record UpdateProfileCommand(
        Guid UserId,
        string? DisplayName,
        string? Bio,
        string? AvatarUrl) : IRequest<UserDto>;
}
