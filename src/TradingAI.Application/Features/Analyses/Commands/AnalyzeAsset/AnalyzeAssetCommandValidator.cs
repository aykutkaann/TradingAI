using FluentValidation;

using TradingAI.Application.Features.Analyses.Commands.AnalyzeImage;
using TradingAI.Application.Features.Analyses.Dtos;

namespace TradingAI.Application.Features.Analyses.Commands.AnalyzeAsset
{
    public class AnalyzeAssetCommandValidator :AbstractValidator<AnalyzeAssetCommand>
    {
        private readonly string[] _allowedTimeFrames = { "1m", "5m", "15m", "30m", "1h", "4h", "1d" };

        public AnalyzeAssetCommandValidator()
        {
            RuleFor(x => x.AssetId).NotEmpty();
            RuleFor(x => x.TimeFrame).Must(timeFrame => _allowedTimeFrames.Contains(timeFrame))
                                     .WithMessage("Invalid time frame.");
            RuleFor(x => x.UserPrompt).MaximumLength(1000);
        }
    }
}
