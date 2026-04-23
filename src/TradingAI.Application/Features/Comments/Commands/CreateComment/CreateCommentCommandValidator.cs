using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.Comments.Commands.CreateComment
{
    public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
    {
        public CreateCommentCommandValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().MinimumLength(1).MaximumLength(1000).Must(s => !string.IsNullOrEmpty(s))
                .WithMessage("Comment cannot be whitespace only.");
            ;
        }
    }
}
