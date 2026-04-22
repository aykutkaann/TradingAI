using MediatR;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Auth.DTOs;
using TradingAI.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TradingAI.Application.Auth.Commands.LoginUser
{
    public class LoginHandler
        (IApplicationDbContext db, IPasswordHasher hasher, IJwtService jwt): IRequestHandler<LoginCommand, AuthResponse>
    {

        public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken ct)
        {
            var user = await db.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Email == request.Email, ct);

            if(user == null || user.IsActive == false)
            {
                throw new NotFoundException("Invalid Credentials");
            }

            var passwordHasher = hasher.Verify(request.Password, user.PasswordHash);
            if (!passwordHasher)
            {
                throw new NotFoundException("Invalid credentials");
            }

            user.LastLoginAt = DateTime.UtcNow;

            var accessToken = jwt.GenerateAccessToken(user);

            var refreshToken = jwt.GenerateRefreshToken(request.IpAddress);

            user.RefreshTokens.Add(refreshToken);

            await db.SaveChangesAsync(ct);


            return new AuthResponse(
                accessToken,
                refreshToken.Token,
                new UserDto(user.Id, user.Email, user.UserName, user.DisplayName, user.AvatarUrl, user.Role));

        }
    }
}
