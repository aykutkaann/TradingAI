using Microsoft.EntityFrameworkCore;
using TradingAI.Domain.Entities;

namespace TradingAI.Application.Common.Interfaces
{
    public interface IApplicationDbContext  
    {
        DbSet<User> Users { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<Asset> Assets { get; }
        DbSet<WatchListItem> WatchListItems { get; }


        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
            
        

    }
}
