

using TradingAI.Domain.Enums;

namespace TradingAI.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string? DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public string Bio { get; set; }
        public UserRole Role  { get; set; }
        public int AnalysisCountThisMonth { get; set; } = 0;


        public DateTime LastAnalysisResetDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }


        public bool IsActive { get; set; } = true;

        //Nav
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();
        public ICollection<WatchListItem> WathcLists { get; set; } = new HashSet<WatchListItem>();
        public ICollection<Analysis> Analyses { get; set; } = new HashSet<Analysis>();
        public ICollection<UserFollow> Followers { get; set; } = new HashSet<UserFollow>();
        public ICollection<UserFollow> Following { get; set; } = new HashSet<UserFollow>();




    }
}
