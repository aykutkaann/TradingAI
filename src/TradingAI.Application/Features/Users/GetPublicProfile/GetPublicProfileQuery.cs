using MediatR;

using TradingAI.Application.Features.Users.DTOs;

namespace TradingAI.Application.Features.Users.GetPublicProfile
{

    public record GetPublicProfileQuery(Guid UserId, Guid? CurrentUserId) : IRequest<PublicProfileDto>;
}
