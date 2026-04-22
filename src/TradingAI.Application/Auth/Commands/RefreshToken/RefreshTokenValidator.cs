using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Auth.Commands.RefreshToken
{
    public class RefreshTokenValidator :AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenValidator()
        {
            RuleFor(x => x.Token).NotEmpty();

        }
    }
}
