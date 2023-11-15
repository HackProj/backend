namespace RatFriendBackend.Persistence.Models;

public class User
{
    public ulong Id { get; set; }
    public string ApiToken { get; set; } = null!;
    public ulong LastPlayTime { get; set; }
    public long TelegramId { get; set; }
    public IList<UserFriendActivity>? FriendActivities { get; set; }
}