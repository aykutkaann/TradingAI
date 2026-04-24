using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Entities;

namespace TradingAI.Infrastructure.Configurations
{
    public class AnalysisConfiguration : IEntityTypeConfiguration<Analysis>
    {
        public void Configure(EntityTypeBuilder<Analysis> builder)
        {
            builder.ToTable("analyses");

            builder.HasKey(a => a.Id);
            builder.HasIndex(a => a.IsPublished);
            builder.HasIndex(a => a.CreatedAt);

            builder.Property(a => a.TimeFrame).IsRequired().HasMaxLength(10);

            builder.Property(a => a.TrendDirection).HasMaxLength(20);

            builder.Property(a => a.DetectedPatterns).HasColumnType("jsonb");

            builder.Property(a => a.KeyLevels).HasColumnType("jsonb");

            builder.Property(a => a.Summary).HasMaxLength(500);

            builder.Property(a => a.ResolvedPrice).HasPrecision(18, 8);

            builder.HasIndex(a => new { a.Outcome, a.OutcomeCheckedAt });

            builder.HasIndex(a => new { a.Outcome, a.ExpiresAt });

            builder.Property(a => a.Pair).IsRequired().HasMaxLength(20);

            builder.Property(a => a.AiAnalysis).HasColumnType("text");

            builder.Property(a => a.Outcome)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasOne(a => a.User)
                .WithMany(u => u.Analyses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Asset)
                .WithMany()
                .HasForeignKey(a => a.AssetId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
