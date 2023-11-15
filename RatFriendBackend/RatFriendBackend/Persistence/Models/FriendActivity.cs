namespace RatFriendBackend.Persistence.Models;

public class FriendActivity
{
    public long Id { get; set; }
    public ulong FriendId { get; set; }
    public string FriendName { get; set; }
    public uint AppId { get; set; }
    public uint TimePlayed { get; set; }
    public string AppName { get; set; } = null!;
    public IList<FriendActivitySubscription>? FriendActivitySubscriptions { get; set; }
}