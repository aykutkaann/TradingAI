
namespace TradingAI.Domain.Entities
{
    public class AnalysisLike
    {
        public Guid Id { get; set; }
        public Guid AnalysisId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public Analysis Analysis { get; set; }

        public User User { get; set; }

    }
}
