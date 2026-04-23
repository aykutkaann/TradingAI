using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.UserFollow.DTOs
{


    public record UserSummaryDto(
        Guid Id,
        string UserName,
        string? DisplayName,
        string? AvatarUrl,
        string? Bio,
        int FollowerCount,
        int FollowingCount,
        bool IsFollowedByMe     
    );
}
