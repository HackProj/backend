using RatFriendBackend.Responses;
using Refit;

namespace RatFriendBackend.HttpClients;

public interface ISteamApiRefit
{
    [Get("/IPlayerService/GetRecentlyPlayedGames/v0001/?key={apiKey}&steamid={steamId}&format=json")]
    Task<RecentlyPlayedGamesRoot> GetRecentlyPlayedGamesAsync(string apiKey, ulong steamId);
}