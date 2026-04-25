using Microsoft.EntityFrameworkCore;
using TradingAI.Domain.Entities;

namespace TradingAI.Application.Common.Interfaces
{
    public interface IApplicationDbContext  
    {
        DbSet<User> Users { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<Analysis> Analyses { get; }
        DbSet <Asset> Assets { get; }
        DbSet<UserFollow> UserFollows { get; }
        DbSet <WatchListItem> WatchLists { get; }
        DbSet<AnalysisLike> AnalysisLikes { get; }
        DbSet<AnalysisComment> AnalysisComments { get; }

        DbSet<SubscriptionPlan> SubscriptionPlans { get; }

        DbSet<UserSubscription> UserSubscriptions { get; }

        DbSet<Notification> Notifications { get; }
    

            Task<int> SaveChangesAsync(CancellationToken cancellationToken);
            
        

    }
}
