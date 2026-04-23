using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.Users.Commands.UpdateProfile
{
    public class UpdateProfileValidator :AbstractValidator<UpdateProfileCommand>
    {

        public UpdateProfileValidator()
        {
            RuleFor(x => x.DisplayName).MaximumLength(50);
            RuleFor(x => x.Bio).MaximumLength(500);
            RuleFor(x => x.AvatarUrl).MaximumLength(500);
        }
    }
}
