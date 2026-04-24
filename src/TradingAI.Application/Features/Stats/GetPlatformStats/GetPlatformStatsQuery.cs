using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Features.Stats.DTOs;

namespace TradingAI.Application.Features.Stats.GetPlatformStats
{
    public record GetPlatformStatsQuery : IRequest<PlatformStatsDto>;

}
