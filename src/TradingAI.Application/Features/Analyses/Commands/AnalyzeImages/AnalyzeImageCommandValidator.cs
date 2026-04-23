using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Features.Analyses.Commands.AnalyzeAsset
{
    public class AnalyzeImageCommandValidator :AbstractValidator<AnalyzeImageCommand>
    {
        private readonly string[] _allowedContentTypes = { "image/png", "image/jpeg", "image/webp" };
        public AnalyzeImageCommandValidator()
        {
            RuleFor(x => x.AssetPair).NotEmpty().MaximumLength(20);

            RuleFor(x => x.TimeFrame).NotEmpty().MaximumLength(10);

            RuleFor(x => x.UserPrompt).MaximumLength(1000);

            RuleFor(x => x.ImageMediaType).Must(x => _allowedContentTypes.Contains(x));

            RuleFor(x => x.FileName).NotEmpty();
        }
    }
}
