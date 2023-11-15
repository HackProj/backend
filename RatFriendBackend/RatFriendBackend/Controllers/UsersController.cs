using Microsoft.AspNetCore.Mvc;
using RatFriendBackend.Requests;
using Steam.Models.SteamCommunity;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace RatFriendBackend.Controllers;

[Microsoft.AspNetCore.Components.Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("playerSummaries")]
    public async Task<IReadOnlyCollection<PlayerSummaryModel>> GetPlayerSummariesAsync(
        GetUserProfileRequest request)
    {
        SteamWebInterfaceFactory steamWebInterfaceFactory = new(request.SteamApiKey);
        var steamClient = steamWebInterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());
        return (await steamClient.GetPlayerSummariesAsync(new[] { request.SteamId })).Data;
    }

    [HttpGet("recentlyPlayedGames")]
    public async Task<RecentlyPlayedGamesResultModel> GetRecentlyPlayedGamesAsync(GetUserProfileRequest request)
    {
        SteamWebInterfaceFactory steamWebInterfaceFactory = new(request.SteamApiKey);
        var steamClient = steamWebInterfaceFactory.CreateSteamWebInterface<PlayerService>(new HttpClient());
        return (await steamClient.GetRecentlyPlayedGamesAsync(request.SteamId)).Data;
    }

    [HttpGet("friendList")]
    public async Task<IReadOnlyCollection<FriendModel>> GetFriendsListAsync(GetUserProfileRequest request)
    {
        SteamWebInterfaceFactory steamWebInterfaceFactory = new(request.SteamApiKey);
        var steamClient = steamWebInterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());
        return (await steamClient.GetFriendsListAsync(request.SteamId)).Data;
    }
}