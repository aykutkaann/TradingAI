using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Features.Analyses.Dtos;

namespace TradingAI.Application.Features.Analyses.Commands.AnalyzeImage
{

    public record AnalyzeAssetCommand(
        Guid UserId,
        Guid AssetId,
        string TimeFrame,   // interval passed to IMarketDataService
        string? UserPrompt
    ) : IRequest<AnalysisDto>;
}
