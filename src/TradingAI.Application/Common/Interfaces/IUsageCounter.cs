using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Application.Common.Interfaces
{
    public interface IUsageCounter
    {
        Task<int> CountAnalysesTodayAsync(Guid userId, CancellationToken ct = default);
        Task<int> CountPublishesTodayAsync(Guid userId, CancellationToken ct = default);
    }
}
