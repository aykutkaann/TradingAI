using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Auth.Commands.RevokeToken
{

    public record RevokeTokenCommand(string Token, string IpAddress) : IRequest;
}
