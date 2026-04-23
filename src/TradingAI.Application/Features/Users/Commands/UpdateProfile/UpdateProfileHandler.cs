using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Auth.DTOs;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Features.Users.Commands.UpdateProfile
{
    public class UpdateProfileHandler(IApplicationDbContext db) :IRequestHandler<UpdateProfileCommand, UserDto>
    {

        public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
                throw new NotFoundException("User not found.");

            if (request.DisplayName != null)
                user.DisplayName = request.DisplayName;

            if (request.Bio != null)
                user.Bio = request.Bio;

            if (request.AvatarUrl != null)
                user.AvatarUrl = request.AvatarUrl;

            await db.SaveChangesAsync(cancellationToken);

            return new UserDto(
                user.Id, user.Email, user.UserName, user.DisplayName, user.AvatarUrl, user.Role);
                



        }
    }
}
