using RatFriendBackend.Responses;
using Refit;

namespace RatFriendBackend.HttpClients;

public interface ISteamApiRefit
{
    /*[Get("/ISteamUser/GetFriendList/v0001/?key={apiKey}&steamid={steamId}&relationship=friend")]
    Task<PlayerFriendsResponse> GetFriendListAsync(string apiKey, long steamId);

    [Get("/ISteamUser/GetPlayerSummaries/v0002/?key={apiKey}&steamids={steamId}")]
    Task<PlayerSummaryRoot> GetPlayerSummariesAsync(string apiKey, long steamId); */
    
    [Get("/IPlayerService/GetOwnedGames/v0001/?key={apiKey}&steamid={steamId}&format=json")]
    Task<string> GetOwnedGamesAsync(string apiKey, ulong steamId);
    
    [Get("/IPlayerService/GetRecentlyPlayedGames/v0001/?key={apiKey}&steamid={steamId}&format=json")]
    Task<RecentlyPlayedGamesRoot> GetRecentlyPlayedGamesAsync(string apiKey, ulong steamId);
}