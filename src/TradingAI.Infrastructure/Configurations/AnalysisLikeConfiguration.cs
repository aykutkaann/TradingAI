using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradingAI.Domain.Entities;

namespace TradingAI.Infrastructure.Configurations
{
    public class AnalysisLikeConfiguration :IEntityTypeConfiguration<AnalysisLike>
    {
        public void Configure(EntityTypeBuilder<AnalysisLike> builder)
        {
            builder.ToTable("analysis_like");

            builder.HasKey(a => a.Id);

            builder.HasIndex(a => new { a.AnalysisId, a.UserId }).IsUnique();

            builder.HasOne(a => a.Analysis)
                .WithMany()
                .HasForeignKey(a => a.AnalysisId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
