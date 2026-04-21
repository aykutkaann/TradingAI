using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Auth.DTOs;

namespace TradingAI.Application.Auth.Commands.RegisterUser
{

    public record RegisterUserCommand(string Email, string UserName, string Password, string IpAddress) : IRequest<AuthResponse>;
}
