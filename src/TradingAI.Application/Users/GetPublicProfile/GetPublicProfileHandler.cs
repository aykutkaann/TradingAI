using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;
using TradingAI.Application.Auth.DTOs;
using TradingAI.Application.Common.Interfaces;

namespace TradingAI.Application.Users.GetPublicProfile
{
    public class GetPublicProfileHandler(IApplicationDbContext db) :IRequestHandler<GetPublicProfileQuery, PublicProfileDto>
    {


        public async Task<PublicProfileDto> Handle(GetPublicProfileQuery request, CancellationToken cancellationToken)
        {
            var profile = await db.Users.AsNoTracking().Where(u => u.UserName == request.UserName && u.IsActive)
                                         .Select(u => new PublicProfileDto(
                                             u.Id, u.UserName, u.DisplayName, u.AvatarUrl, u.Bio, u.Role, u.CreatedAt))
                                         .FirstOrDefaultAsync(cancellationToken);

            if (profile == null)
                throw new NotFoundException("Profile not found.");

            return profile;
    
        }
    }
}
