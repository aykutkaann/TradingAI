using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Auth.DTOs;

namespace TradingAI.Application.Auth.Commands.LoginUser
{
    public record LoginCommand(string Email, string Password, string IpAddress) : IRequest<AuthResponse>;
    
    
}
