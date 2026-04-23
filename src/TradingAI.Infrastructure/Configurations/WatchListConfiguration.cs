using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Entities;

namespace TradingAI.Infrastructure.Configurations
{

    public class WatchListConfiguration : IEntityTypeConfiguration<WatchListItem>
    {
        public void Configure(EntityTypeBuilder<WatchListItem> builder)
        {
            builder.ToTable("watch_lists");

            builder.HasKey(w => w.Id);

            // A user can only add each asset once (composite unique),
            // but the same asset can appear in many users' watchlists.
            builder.HasIndex(w => new { w.UserId, w.AssetId }).IsUnique();

            builder.HasOne(w => w.User)
                .WithMany(u => u.WathcLists)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(w => w.Asset)
                .WithMany()
                .HasForeignKey(w => w.AssetId)
                .OnDelete(DeleteBehavior.Restrict);



        }
    }
}
