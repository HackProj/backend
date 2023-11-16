namespace RatFriendBackend.Persistence.Models;

public class FriendActivitySubscription
{
    public long Id { get; set; }
    public long UserSubscriptionId { get; set; }
    public UserSubscription? UserSubscription { get; set; }
    public long FriendActivityId { get; set; }
    public FriendActivity? FriendActivity { get; set; }
}