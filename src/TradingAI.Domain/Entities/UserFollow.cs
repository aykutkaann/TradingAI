namespace TradingAI.Domain.Entities;

public class UserFollow
{
    public Guid Id { get; set; }
    public Guid FollowerId { get; set; }       // who is following
    public Guid FollowingId { get; set; }      // who is being followed
    public DateTime CreatedAt { get; set; }

    public User Follower { get; set; } = null!;
    public User Following { get; set; } = null!;
}
