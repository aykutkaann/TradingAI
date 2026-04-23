using MediatR;
using TradingAI.Application.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingAI.API.Controllers.Requests;
using TradingAI.Application.Features.Analyses.Dtos;
using TradingAI.Application.Features.Analyses.Commands.AnalyzeImage;
using TradingAI.API.Extensions;
using TradingAI.Application.Features.Analyses.Commands.AnalyzeAsset;

namespace TradingAI.API.Controllers
{
    [ApiController]
    [Route("api/analysis")]
    [Authorize]
    public class AnalysisController(ISender mediator) : ControllerBase
    {
        [HttpPost("image")]
        [RequestSizeLimit(10*1024*1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        public async Task<ActionResult<AnalysisDto>> AnalyzeImage([FromForm] AnalyzeImageRequest body, CancellationToken ct)
        {
            if (body.File is null || body.File.Length == 0)
                throw new ValidationException();

            await using var stream = body.File.OpenReadStream();

            var cmd = new AnalyzeImageCommand(
                UserId: User.GetUserId(),
                ImageStream: stream,
                FileName: body.File.FileName,
                ImageMediaType: body.File.ContentType,
                AssetPair: body.AssetPair,
                TimeFrame: body.TimeFrame,
                UserPrompt: body.UserPrompt);

            var result = await mediator.Send(cmd, ct);

            return Ok(result);
        }


        [HttpPost("assets")]
        public async Task<ActionResult<AnalysisDto>> AnalyzeAsset([FromBody] AnalyzeAssetRequest body, CancellationToken ct)
        {
            var cmd = new AnalyzeAssetCommand(
                UserId: User.GetUserId(),
                AssetId:body.AssetId,
                TimeFrame:body.TimeFrame,
                UserPrompt:body.UserPrompt
                );

            var result = await mediator.Send(cmd, ct);

            return Ok(result);
        }
    }
}
