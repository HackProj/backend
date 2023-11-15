namespace RatFriendBackend.Responses;

public class Game
{
    public ulong appid { get; set; }
    public string name { get; set; }
    public ulong playtime_2weeks { get; set; }
    public ulong playtime_forever { get; set; }
    public string img_icon_url { get; set; }
    public ulong playtime_windows_forever { get; set; }
    public ulong playtime_mac_forever { get; set; }
    public ulong playtime_linux_forever { get; set; }
}

public class RecentlyPlayedGamesResponse
{
    public List<Game> Games { get; set; }
}

public class RecentlyPlayedGamesRoot
{
    public RecentlyPlayedGamesResponse RecentlyPlayedGamesResponse { get; set; }
}