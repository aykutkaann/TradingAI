using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Enums;

namespace TradingAI.Application.Auth.DTOs
{

    public record PublicProfileDto(
        Guid Id,
        string Username,
        string? DisplayName,
        string? AvatarUrl,
        string? Bio,
        UserRole Role,
        DateTime CreatedAt
);
}
