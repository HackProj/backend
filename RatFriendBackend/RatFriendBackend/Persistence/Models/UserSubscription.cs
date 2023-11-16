namespace RatFriendBackend.Persistence.Models;

public class UserSubscription
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public ulong FriendId { get; set; }
    public bool IsFollowing { get; set; }
    public bool WantHint { get; set; }
    public IList<FriendActivitySubscription>? FriendActivitySubscriptions { get; set; }
}