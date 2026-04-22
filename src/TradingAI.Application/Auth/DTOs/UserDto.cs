using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Enums;

namespace TradingAI.Application.Auth.DTOs
{
    public record UserDto(Guid Id, string Email, string UserName, string? DisplayName, string? AvatarUrl, UserRole Role);
    
    
}
