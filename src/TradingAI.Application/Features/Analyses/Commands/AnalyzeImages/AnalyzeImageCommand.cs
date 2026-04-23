using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Features.Analyses.Dtos;

namespace TradingAI.Application.Features.Analyses.Commands.AnalyzeAsset
{
    public record AnalyzeImageCommand(
            Guid UserId,          // from auth context, not client
            Stream ImageStream,
            string FileName,
            string ImageMediaType,
            string AssetPair,     // e.g. "BTC/USDT" — free text from user
            string TimeFrame,     // "1H", "4H", "1D" etc.
            string? UserPrompt) : IRequest<AnalysisDto>;
            
    
}
