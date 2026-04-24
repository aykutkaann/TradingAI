using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingAI.API.Controllers.Requests;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Application.Features.Subscriptions.Commands.CancelSubscription;
using TradingAI.Application.Features.Subscriptions.Commands.UpgradeSubscription;
using TradingAI.Application.Features.Subscriptions.DTOs;
using TradingAI.Application.Features.Subscriptions.Queries.GetMySubscription;

namespace TradingAI.API.Controllers
{
    [ApiController]
    [Route("api/subscription")]
    [Authorize]
    public class SubscriptionController(ICurrentUserService currentUser, IMediator mediator) : ControllerBase
    {
        [HttpGet("me")]
        public async Task<ActionResult<SubscriptionDto>> GetSubs(CancellationToken ct)
        {
            var userId =  currentUser.RequireUserId();

            var result = await mediator.Send(new GetMySubscriptionQuery(userId), ct);

            return Ok(result);
        }

        [HttpPost("upgrade")]
        public async Task<ActionResult<SubscriptionDto>> UpgradeSubs([FromBody] UpgradeSubscriptionRequest request,CancellationToken ct)
        {
            var userId = currentUser.RequireUserId();

            var cmd = new UpgradeSubscriptionCommand(userId, request.NewTier);

            var result = await mediator.Send(cmd, ct);

            return Ok(result);
        }

        [HttpPost("cancel")]
        public async Task<ActionResult<SubscriptionDto>> CancelSubs(CancellationToken ct)
        {
            var userId = currentUser.RequireUserId();

            var result =  await mediator.Send(new CancelSubscriptionCommand(userId), ct);

            return Ok(result);


        }
    }
}
