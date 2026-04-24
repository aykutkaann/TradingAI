using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Entities;

namespace TradingAI.Infrastructure.Configurations
{
    public class SubscriptionPlanConfiguration :IEntityTypeConfiguration<SubscriptionPlan>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
        {
            builder.ToTable("subscription_plans");

            builder.HasKey(s => s.Id);

            builder.HasIndex(s => s.Tier).IsUnique();

            builder.Property(s => s.Tier).HasConversion<string>();

            builder.Property(s => s.Name).IsRequired().HasMaxLength(30);

            builder.Property(s => s.PriceWeeklyUsd).HasPrecision(10, 2);
            builder.Property(s => s.PriceMonthlyUsd).HasPrecision(10, 2);
            builder.Property(s => s.PriceYearlyUsd).HasPrecision(10, 2);

        }
    }
}
