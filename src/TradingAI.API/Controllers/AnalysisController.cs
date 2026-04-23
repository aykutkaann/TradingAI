using MediatR;
using TradingAI.Application.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingAI.API.Controllers.Requests;
using TradingAI.Application.Features.Analyses.Dtos;
using TradingAI.Application.Features.Analyses.Commands.AnalyzeImage;
using TradingAI.API.Extensions;
using TradingAI.Application.Features.Analyses.Commands.AnalyzeAsset;
using TradingAI.Application.Common.Models;
using TradingAI.Application.Features.Analyses.Queries.GetMyAnalyses;
using TradingAI.Application.Features.Analyses.Queries.GetAnalysisById;
using TradingAI.Application.Features.Analyses.Commands.DeleteAnalysis;
using TradingAI.Application.Features.Analyses.Commands.PublishAnalysis;
using TradingAI.Application.Features.Analyses.Commands.UnpublishAnalysis;

namespace TradingAI.API.Controllers
{
    [ApiController]
    [Route("api/analysis")]
    [Authorize]
    public class AnalysisController(ISender mediator) : ControllerBase
    {

        //Analyze with just image

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


        //Analyze with just asset
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


        //Get all analyses
        [HttpGet]
        public async Task<ActionResult<PagedResult<AnalysisDto>>> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var result = await mediator.Send(new GetMyAnalysesQuery(User.GetUserId(), page, pageSize), ct);

            return Ok(result);
        }

        //Getby ID

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<AnalysisDto>> GetById(Guid id, CancellationToken ct = default)
        {
            Guid? currentUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;

            var result = await mediator.Send(new GetAnalysisByIdQuery(id, currentUserId), ct);

            return Ok(result);
        }

        //Delete Analysis

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await mediator.Send(new DeleteAnalysisCommand(id, User.GetUserId()), ct);

            return NoContent();
        }

        //Publish

        [HttpPut("{id:guid}/publish")]
        public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
        {
            await mediator.Send(new PublishAnalysisCommand(id, User.GetUserId()), ct);

            return NoContent();
        }

        //Unpublish

        [HttpPut("{id:guid}/unpublish")]
        public async Task<IActionResult> Unpublish(Guid id, CancellationToken ct)
        {

            await mediator.Send(new UnpublishAnalysisCommand(id, User.GetUserId()), ct);

            return NoContent();
        }


    }
}
