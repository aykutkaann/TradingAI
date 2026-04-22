using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Auth.DTOs;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Domain.Entities;

namespace TradingAI.Application.Auth.Commands.RefreshToken
{
    public class RefreshTokenHandler(IApplicationDbContext db, IJwtService jwt) :IRequestHandler<RefreshTokenCommand, AuthResponse>
    {

        public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
        {

            var user = await db.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == request.Token), ct);

            var oldToken = user?.RefreshTokens.Single(t => t.Token == request.Token);


            if(user == null || oldToken == null)
            {
                throw new NotFoundException("Invalid Token");

            }

            if(!oldToken.IsActive || oldToken.IsExpired || oldToken.IsRevoked)
            {
                throw new UnauthorizedException("Unauthorized");
            }

            oldToken.RevokedAt = DateTime.UtcNow;
            oldToken.RevokedByIp = request.IpAddress;

            var newToken = jwt.GenerateRefreshToken(request.IpAddress);
            var newAccessToken = jwt.GenerateAccessToken(user);

            oldToken.ReplacedByToken = newToken.Token;
            user.RefreshTokens.Add(newToken);

            await db.SaveChangesAsync(ct);

            return new AuthResponse(
                newAccessToken,
                newToken.Token,
                new UserDto(user.Id, user.Email, user.UserName, user.DisplayName, user.AvatarUrl, user.Role));

        }
    }
}
