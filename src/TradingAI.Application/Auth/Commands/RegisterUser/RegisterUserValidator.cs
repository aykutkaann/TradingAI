using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Auth.Commands.RegisterUser
{
    public class RegisterUserValidator :AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserValidator()
        {
            RuleFor(x => x.Email).EmailAddress().NotEmpty().MaximumLength(256);

            RuleFor(x => x.UserName).NotEmpty().MinimumLength(3).MaximumLength(50)
                            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores");
            
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        }
    }
}
