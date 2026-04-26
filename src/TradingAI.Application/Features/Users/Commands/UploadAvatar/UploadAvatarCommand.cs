using MediatR;
using TradingAI.Application.Auth.DTOs;

namespace TradingAI.Application.Features.Users.Commands.UploadAvatar;

public record UploadAvatarCommand(
    Guid UserId,
    Stream Stream,
    string FileName,
    string ContentType
) : IRequest<UserDto>;
