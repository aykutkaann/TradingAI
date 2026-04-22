using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Auth.Commands.RevokeToken;

public class RevokeTokenHandler(IApplicationDbContext db)
    : IRequestHandler<RevokeTokenCommand>
{
    public async Task Handle(RevokeTokenCommand request, CancellationToken ct)
    {
        var token = await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.Token, ct);

        if (token is null)
            throw new NotFoundException("Invalid token");

        if (!token.IsActive)
            throw new UnauthorizedException("Token already revoked or expired");

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = request.IpAddress;

        await db.SaveChangesAsync(ct);
    }
}
