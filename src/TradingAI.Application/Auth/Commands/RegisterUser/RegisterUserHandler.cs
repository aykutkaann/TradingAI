using MediatR;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Auth.DTOs;
using TradingAI.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using TradingAI.Domain.Entities;
using TradingAI.Domain.Enums;

namespace TradingAI.Application.Auth.Commands.RegisterUser
{
    public class RegisterUserHandler(
        IApplicationDbContext db, IPasswordHasher hasher, IJwtService jwt) :IRequestHandler<RegisterUserCommand, AuthResponse>
    {
        public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken ct)
        {
            var isEmailTaken = await db.Users.AnyAsync(u => u.Email == request.Email, ct);

            if (isEmailTaken)
                throw new ConflictException("Email is alteady taken.");

            var isUsernameTaken = await db.Users.AnyAsync(u => u.UserName == request.UserName, ct);
            if (isUsernameTaken)
                throw new ConflictException("User name is already taken");

            var hashedPassword = hasher.Hash(request.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = request.UserName,
                Email = request.Email,
                PasswordHash = hashedPassword,
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var accessToken = jwt.GenerateAccessToken(user);

            var refreshToken = jwt.GenerateRefreshToken(request.IpAddress);

            user.RefreshTokens.Add(refreshToken);

            db.Users.Add(user);

            var freePlan = await db.SubscriptionPlans.FirstOrDefaultAsync(p => p.Tier == SubscriptionTier.Free && p.IsActive, ct)
                ?? throw new InvalidOperationException("Free plan not seeded.");

            db.UserSubscriptions.Add(new UserSubscription
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                PlanId = freePlan.Id,
                Tier = SubscriptionTier.Free,
                StartedAt = DateTime.UtcNow,
                ExpiresAt =null,
                IsActive = true,

            });

            await db.SaveChangesAsync(ct);

            return new AuthResponse(
                accessToken,
                refreshToken.Token,
                new UserDto(user.Id,user.Email, user.UserName, user.DisplayName, user.AvatarUrl, user.Role));
         

        }

    }
}
