using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Auth.DTOs;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Features.Users.Commands.UploadAvatar;

public class UploadAvatarHandler(IApplicationDbContext db, IFileStorage storage)
    : IRequestHandler<UploadAvatarCommand, UserDto>
{
    private static readonly string[] AllowedContentTypes =
        { "image/png", "image/jpeg", "image/jpg", "image/webp", "image/gif" };

    public async Task<UserDto> Handle(UploadAvatarCommand request, CancellationToken ct)
    {
        if (!AllowedContentTypes.Contains(request.ContentType.ToLowerInvariant()))
            throw new ValidationException();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
                   ?? throw new NotFoundException("User not found.");

        // Best-effort cleanup of the previous avatar so we don't leak files.
        if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
        {
            try { await storage.DeleteAsync(user.AvatarUrl, ct); } catch { /* swallow */ }
        }

        var url = await storage.SaveAsync(request.Stream, request.FileName, request.UserId.ToString(), ct);
        user.AvatarUrl = url;
        await db.SaveChangesAsync(ct);

        return new UserDto(user.Id, user.Email, user.UserName, user.DisplayName, user.AvatarUrl, user.Role);
    }
}
