using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Auth.DTOs;

namespace TradingAI.Application.Auth.Commands.RefreshToken
{
    public record RefreshTokenCommand(string Token, string IpAddress) : IRequest<AuthResponse>;
    
    
}
