using MediatR;
using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Exceptions;

using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Features.Users.DTOs;

namespace TradingAI.Application.Features.Users.GetPublicProfile
{
    public class GetPublicProfileHandler(IApplicationDbContext db) :IRequestHandler<GetPublicProfileQuery, PublicProfileDto>
    {


        public async Task<PublicProfileDto> Handle(GetPublicProfileQuery request, CancellationToken cancellationToken)
        {
            var profile = await db.Users
                .AsNoTracking()
                .Where(u => u.Id == request.UserId && u.IsActive)
                .Select(u => new PublicProfileDto(
                    u.Id,
                    u.UserName,
                    u.DisplayName,
                    u.AvatarUrl,
                    u.Bio,
                    u.Followers.Count,
                    u.Following.Count,
                    u.Analyses.Count(a => a.IsPublished),
                    request.CurrentUserId != null && u.Followers.Any(f => f.FollowerId == request.CurrentUserId),
                    u.CreatedAt)).FirstOrDefaultAsync(cancellationToken);
       
                    
            if (profile == null)
                throw new NotFoundException("Profile not found.");

            return profile;
    
        }
    }
}
