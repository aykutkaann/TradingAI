using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Domain.Entities
{
    public class WatchListItem
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid AssetId { get; set; }
        public int SortOrder { get; set; }
        public DateTime AddedAt { get; set; }

        // Navigation
        public User User { get; set; }
        public Asset Asset { get; set; }
    }
}
