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


        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
            
        

    }
}
