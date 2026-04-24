using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.Users.DTOs;

public record PublicProfileDto(Guid Id, string UserName, string? DisplayName, string? AvatarUrl, string? Bio,
    int FollowersCount, int FollowingCount, int AnalysesCount, bool IsFollowedByMe, DateTime CreatedAt);
