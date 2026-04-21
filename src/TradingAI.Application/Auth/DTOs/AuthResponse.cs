using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Auth.DTOs
{
    public record AuthResponse(string AccessToken, string RefreshToken, UserDto User);
    
    
}
