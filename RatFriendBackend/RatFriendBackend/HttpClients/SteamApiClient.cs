using Steam.Models.SteamCommunity;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace RatFriendBackend.HttpClients;

public class SteamApiClient : ISteamApiClient
{
    private readonly SteamUser _steamUser;
    private readonly PlayerService _playerService;

    public SteamApiClient()
    {
        SteamWebInterfaceFactory steamWebInterfaceFactory = new(Constants.SteamApiToken);
        _steamUser = steamWebInterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());
        _playerService = steamWebInterfaceFactory.CreateSteamWebInterface<PlayerService>(new HttpClient());
    }

    public async Task<IReadOnlyCollection<PlayerSummaryModel>> GetPlayerSummariesAsync(
        IReadOnlyCollection<ulong> steamIds)
    {
        return (await _steamUser.GetPlayerSummariesAsync(steamIds)).Data;
    }

    public async Task<IReadOnlyCollection<PlayerSummaryModel>> GetPlayerSummariesAsync(ulong steamId)
    {
        return (await _steamUser.GetPlayerSummariesAsync(new[] { steamId })).Data;
    }

    public async Task<ulong> ResolveVanityUrlAsync(string vanityUrl)
    {
        return (await _steamUser.ResolveVanityUrlAsync(vanityUrl)).Data;
    }

    public async Task<IReadOnlyCollection<FriendModel>> GetFriendsListAsync(ulong steamId)
    {
        return (await _steamUser.GetFriendsListAsync(steamId)).Data;
    }

    public async Task<RecentlyPlayedGamesResultModel> GetRecentlyPlayedGamesAsync(ulong steamId)
    {
        return (await _playerService.GetRecentlyPlayedGamesAsync(steamId)).Data;
    }

    public async Task<OwnedGamesResultModel> GetOwnedGamesAsync(ulong steamId)
    {
        return (await _playerService.GetOwnedGamesAsync(steamId, includeFreeGames: true)).Data;
    }
}