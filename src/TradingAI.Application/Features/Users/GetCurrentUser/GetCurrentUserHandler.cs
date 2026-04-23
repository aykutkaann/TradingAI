using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Auth.DTOs;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Features.Users.GetCurrentUser
{
    public class GetCurrentUserHandler(IApplicationDbContext db) : IRequestHandler<GetCurrentUserQuery, UserDto>
    {
        public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                throw new NotFoundException("User not found");


            return new UserDto(
                user.Id, user.Email, user.UserName, user.DisplayName, user.AvatarUrl, user.Role);
        }
    }
}
