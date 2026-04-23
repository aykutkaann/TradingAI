using System;
using System.Collections.Generic;
using System.Text;

namespace TradingAI.Domain.Entities
{
    public class AnalysisComment
    {
        public Guid Id { get; set; }
        public Guid AnalysisId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; }       // Max 500 chars
        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }

        public User User { get; set; }
        public Analysis Analysis { get; set; }

    }
}
