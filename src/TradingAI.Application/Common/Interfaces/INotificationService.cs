using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Enums;

namespace TradingAI.Application.Common.Interfaces
{
    public interface INotificationService
    {

        Task NotifyNewFollowerAsync(Guid recipientUserId, Guid followerUserId, string followerUserName, CancellationToken ct);
        Task NotifyNewLikeAsync(Guid recipientUserId, Guid likerUserId, string likerUserName, Guid analysisId, CancellationToken ct);
        Task NotifyNewCommentAsync(Guid recipientUserId, Guid commenterUserId, string commenterUserName, Guid analysisId, CancellationToken ct);
        Task NotifyAnalysisResolvedAsync(Guid recipientUserId, Guid analysisId, string assetPair, AnalysisOutcome outcome, CancellationToken ct);
    }
}
