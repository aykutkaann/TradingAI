using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Entities;

namespace TradingAI.Infrastructure.Configurations
{
    public class AssetConfiguration : IEntityTypeConfiguration<Asset>
    {
        public void Configure(EntityTypeBuilder<Asset> builder)
        {
            builder.ToTable("assets");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Symbol).IsRequired().HasMaxLength(20);
            builder.Property(a => a.Pair).IsRequired().HasMaxLength(20);

            builder.HasIndex(a => a.Pair).IsUnique();

            builder.Property(a => a.Name).IsRequired().HasMaxLength(100);

            builder.Property(a => a.DataSourceId).IsRequired().HasMaxLength(100);



        }
    }
}
