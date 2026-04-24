using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Entities;

namespace TradingAI.Infrastructure.Configurations
{
    public class UserSubscriptionConfiguration :IEntityTypeConfiguration<UserSubscription>
    {
        public void Configure(EntityTypeBuilder<UserSubscription> builder)
        {
            builder.ToTable("user_subscriptions");

            builder.HasKey(u => u.Id);

            builder.HasIndex(u => new { u.UserId, u.IsActive });

            builder.Property(u => u.Tier).HasConversion<string>();

            builder.HasOne(u => u.User)
                .WithMany(x => x.Subscriptions)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
