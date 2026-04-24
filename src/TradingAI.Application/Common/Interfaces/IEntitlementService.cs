using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Features.Subscriptions.Models;

namespace TradingAI.Application.Common.Interfaces
{
    public interface IEntitlementService
    {
        Task<Entitlements> GetAsync(Guid userId, CancellationToken ct = default);
    }
}
