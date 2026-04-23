using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using TradingAI.Domain.Entities;

namespace TradingAI.Infrastructure.Configurations
{
    public class AnalysisCommentConfiguration :IEntityTypeConfiguration<AnalysisComment>
    {
        public void Configure(EntityTypeBuilder<AnalysisComment> builder)
        {
            builder.ToTable("analysis_comment");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Content).IsRequired().HasMaxLength(500);

            builder.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Analysis)
                .WithMany()
                .HasForeignKey(a => a.AnalysisId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
