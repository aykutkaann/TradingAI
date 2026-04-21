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


        }
    }
}
