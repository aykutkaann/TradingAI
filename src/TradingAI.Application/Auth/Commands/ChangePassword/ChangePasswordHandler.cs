using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Auth.Commands.ChangePassword;

public class ChangePasswordHandler(
    IApplicationDbContext db,
    IPasswordHasher hasher) : IRequestHandler<ChangePasswordCommand, Unit>
{
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
                   ?? throw new NotFoundException("User not found.");

        if (!hasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedException("Current password is incorrect.");

        if (request.CurrentPassword == request.NewPassword)
            throw new ConflictException("New password must differ from current.");

        user.PasswordHash = hasher.Hash(request.NewPassword);
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
