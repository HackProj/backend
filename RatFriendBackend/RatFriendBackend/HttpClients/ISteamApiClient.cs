using Steam.Models.SteamCommunity;

namespace RatFriendBackend.HttpClients;

public interface ISteamApiClient
{
    Task<IReadOnlyCollection<PlayerSummaryModel>> GetPlayerSummariesAsync(IReadOnlyCollection<ulong> steamIds);
    Task<IReadOnlyCollection<PlayerSummaryModel>> GetPlayerSummariesAsync(ulong steamId);
    Task<ulong> ResolveVanityUrlAsync(string vanityUrl);
    Task<RecentlyPlayedGamesResultModel> GetRecentlyPlayedGamesAsync(ulong steamId);
    Task<IReadOnlyCollection<FriendModel>> GetFriendsListAsync(ulong steamId);
    Task<OwnedGamesResultModel> GetOwnedGamesAsync(ulong steamId);
}