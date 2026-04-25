using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Domain.Entities;
using TradingAI.Domain.Enums;

namespace TradingAI.Infrastructure.Notifications
{
    public class NotificationService(IApplicationDbContext db) : INotificationService
    {
        public async Task NotifyNewFollowerAsync(Guid recipientUserId, Guid followerUserId, string followerUserName, CancellationToken ct)
        {
            if (recipientUserId == followerUserId) return; //Cant follow yourself

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = recipientUserId,
                ActorUserId = followerUserId,
                Type = Domain.Enums.NotificationType.NewFollower,
                Title = "New Follower",
                Message = $"{followerUserName} started following you",
                RelatedEntityId = followerUserId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            db.Notifications.Add(notification);

            await db.SaveChangesAsync(ct);
        }

        public async Task NotifyNewLikeAsync(Guid recipientUserId, Guid likerUserId, string likerUserName, Guid analysisId, CancellationToken ct)
        {
            if (recipientUserId == likerUserId) return;

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = recipientUserId,
                ActorUserId = likerUserId,
                Type = Domain.Enums.NotificationType.NewLike,
                Title = "New Like",
                Message = $"{likerUserName} liked your analysis with id {analysisId}",
                RelatedEntityId = analysisId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow


            };

            db.Notifications.Add(notification);
            await db.SaveChangesAsync(ct);
        }

        public async Task NotifyNewCommentAsync(Guid recipientUserId, Guid commenterUserId, string commenterUserName, Guid analysisId, CancellationToken ct)
        {

            if (recipientUserId == commenterUserId) return;

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = recipientUserId,
                ActorUserId = commenterUserId,
                Type = Domain.Enums.NotificationType.NewComment,
                Title = "New Comment",
                Message = $"{commenterUserName} has commented your analysis with id {analysisId}",
                RelatedEntityId = analysisId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            db.Notifications.Add(notification);
            await db.SaveChangesAsync(ct);
        }

        public async Task NotifyAnalysisResolvedAsync(Guid recipientUserId, Guid analysisId, string assetPair, AnalysisOutcome outcome, CancellationToken ct)
        {
            if (outcome == AnalysisOutcome.Pending) return;

            var (title, message) = outcome switch
            {
                AnalysisOutcome.Win => ("Analysis WON", $"Your {assetPair} analysis hit take profit!"),
                AnalysisOutcome.Loss => ("Analysis Loss", $"Your {assetPair} analysis hit stop loss."),
                AnalysisOutcome.Invalidated => ("Analysis invalidated", $"Your {assetPair} analysis was invalidated."),
                AnalysisOutcome.Expired => ("Analysis expired", $"Your {assetPair} analysis expired without resolution."),
                _ => ("Analysis resolved", $"Your {assetPair} analysis was resolved.")
            };


            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = recipientUserId,
                ActorUserId = null,                 // system event, no human actor
                Type = NotificationType.AnalysisResolved,
                Title = title,
                Message = message,
                RelatedEntityId = analysisId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            db.Notifications.Add(notification);
            await db.SaveChangesAsync(ct);

        }

    }
}
