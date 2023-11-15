using System.ComponentModel.DataAnnotations.Schema;

namespace RatFriendBackend.Persistence.Models;

public class FriendActivityInfo
{
    public ulong Id { get; set; }
    public ulong FriendId { get; set; }
    public ulong LastPlayTime { get; set; }
    // Флаг нужен для того, чтобы не отправлять уведомления о том, что друг зашел в игру, если он уже был в ней в прошлый раз
    public bool StillPlayingWithoutUs { get; set; }
    
    [Column(TypeName = "jsonb")] 
    public List<GamesInfo> GameInfos { get; set; } = new();
    public IList<UserFriendActivity>? Users { get; set; }
}

public record GamesInfo(string Name, ulong GameId, ulong PlayTimeForever);