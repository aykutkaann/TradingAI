using Microsoft.EntityFrameworkCore;
using TradingAI.Application.Common.Interfaces;
using TradingAI.Domain.Entities;


namespace TradingAI.Infrastructure
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) :DbContext(options), IApplicationDbContext
    {

        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Analysis> Analyses => Set<Analysis>();
        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<UserFollow> UserFollows => Set<UserFollow>();
        public DbSet<WatchListItem> WatchLists => Set<WatchListItem>();




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
