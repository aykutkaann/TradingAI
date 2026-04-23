using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingAI.Domain.Entities;

namespace TradingAI.Infrastructure.Configurations;

public class UserFollowConfiguration : IEntityTypeConfiguration<UserFollow>
{
    public void Configure(EntityTypeBuilder<UserFollow> builder)
    {
        builder.ToTable("user_follows");

        builder.HasKey(u => u.Id);

        // A user can only follow another user once
        builder.HasIndex(u => new { u.FollowerId, u.FollowingId }).IsUnique();

        builder.HasOne(u => u.Follower)
            .WithMany(u => u.Following)   // my "Following" collection is where I'm the Follower
            .HasForeignKey(u => u.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Following)
            .WithMany(u => u.Followers)   // my "Followers" collection is where I'm the target
            .HasForeignKey(u => u.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
