using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Auth.DTOs;

namespace TradingAI.Application.Features.Users.GetPublicProfile
{

    public record GetPublicProfileQuery(string UserName) : IRequest<PublicProfileDto>;
}
