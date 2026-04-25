using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Entities;

namespace TradingAI.Infrastructure.Configurations
{
    public class NotificationConfiguration :IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("notifications");
            builder.HasKey(n => n.Id);

            builder.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt }).IsDescending(false, false, false);
            builder.HasIndex(n => new { n.UserId, n.CreatedAt }).IsDescending(false, true);


            builder.Property(n => n.Type).HasConversion<string>().IsRequired().HasMaxLength(50);

            builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
            builder.Property(n => n.Message).HasMaxLength(1000).IsRequired();

            builder.Property(n => n.IsRead).HasDefaultValue(false);

            builder.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);


        }
    }
}
