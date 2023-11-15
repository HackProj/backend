namespace RatFriendBackend.Persistence.Models;

public class UserFriendActivity
{
    public ulong UserId { get; set; }
    public User? User { get; set; }
    public ulong FriendId { get; set; }
    public FriendActivityInfo? FriendActivityInfo { get; set; }

    public UserFriendActivity(ulong userId, ulong friendId)
    {
        UserId = userId;
        FriendId = friendId;
    }
}