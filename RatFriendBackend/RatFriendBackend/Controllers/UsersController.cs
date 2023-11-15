using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RatFriendBackend.HttpClients;
using RatFriendBackend.Persistence;
using RatFriendBackend.Persistence.Models;
using RatFriendBackend.Requests;
using Steam.Models.SteamCommunity;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace RatFriendBackend.Controllers;

[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ISteamApiClient _steamApiClient;

    public UsersController(ApplicationDbContext context, ISteamApiClient steamApiClient)
    {
        _context = context;
        _steamApiClient = steamApiClient;
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe(SubscribeToUserRequest request)
    {
        var userName = GetUserNameFromUrl(request.FriendUrl);
        if (userName == null)
            return BadRequest("Invalid friend url");

        var friendId = await _steamApiClient.ResolveVanityUrlAsync(userName);
        await CreateUserSubscriptionIfNotExists(friendId, request.TelegramId);

        var appName = GetAppNameFromUrl(request.AppUrl);
        if (appName == null)
            return BadRequest("Invalid app url");

        var appId = GetAppIdFromUrl(request.AppUrl);
        if (appId == null)
            return BadRequest("Invalid app url");

        var errorMsg = await CreateFriendActivitiesIfNotExists(appId.Value, appName, friendId);
        if (errorMsg != null)
            return BadRequest(errorMsg);

        await _context.SaveChangesAsync();
        await CreateFriendActivitySubscriptionIfNotExists(friendId, request.TelegramId, appId.Value);
        await _context.SaveChangesAsync();

        return Ok();
    }

    private async Task CreateUserSubscriptionIfNotExists(ulong friendId, long telegramId)
    {
        if (await _context.UserSubscription.FirstOrDefaultAsync(
                x => x.FriendId == friendId
                     && x.UserId == telegramId) == null)
        {
            _context.UserSubscription.Add(new UserSubscription()
            {
                FriendId = friendId,
                UserId = telegramId,
                IsFollowing = true
            });
        }
    }

    private async Task<string?> CreateFriendActivitiesIfNotExists(uint appId, string appName, ulong friendId)
    {
        if (await _context.FriendActivities.FirstOrDefaultAsync(x =>
                x.FriendId == friendId && x.AppId == appId) != null)
            return null;

        var app = await GetRecentlyPlayedGamesAsync(friendId, appId);
        if (app == null)
            return "User doesn't own this game";

        var friendName = (await _steamApiClient.GetPlayerSummariesAsync(friendId)).First().Nickname;
        _context.FriendActivities.Add(new FriendActivity()
        {
            FriendId = friendId,
            FriendName = friendName,
            AppName = appName,
            AppId = appId,
            TimePlayed = app.PlaytimeForever
        });

        return null;
    }

    private async Task CreateFriendActivitySubscriptionIfNotExists(ulong friendId, long telegramId, ulong appId)
    {
        if (await _context.FriendActivitySubscriptions.FirstOrDefaultAsync(x =>
                x.UserSubscription!.UserId == telegramId
                && x.UserSubscription.FriendId == friendId
                && x.FriendActivity!.FriendId == friendId
                && x.FriendActivity.AppId == appId) == null)
        {
            await _context.UserSubscription.FirstAsync(x =>
                x.UserId == telegramId && x.FriendId == friendId);
            _context.FriendActivitySubscriptions.Add(new FriendActivitySubscription
            {
                UserSubscription = await _context.UserSubscription.FirstAsync(x =>
                    x.UserId == telegramId && x.FriendId == friendId),
                FriendActivity = await _context.FriendActivities.FirstAsync(x =>
                    x.FriendId == friendId && x.AppId == appId)
            });
        }
    }

    private static async Task<RecentlyPlayedGameModel?> GetRecentlyPlayedGamesAsync(ulong friendId, ulong appId)
    {
        SteamWebInterfaceFactory steamWebInterfaceFactory = new(Constants.SteamApiToken);
        var steamClient = steamWebInterfaceFactory.CreateSteamWebInterface<PlayerService>(new HttpClient());
        return (await steamClient.GetRecentlyPlayedGamesAsync(friendId)).Data.RecentlyPlayedGames.FirstOrDefault(x =>
            x.AppId == appId);
    }

    private uint? GetAppIdFromUrl(string appUrl)
    {
        Uri uri = new Uri(appUrl);
        if (!uint.TryParse(uri.Segments[^2].Trim('/'), out var result))
            return null;
        return result;
    }

    private string? GetAppNameFromUrl(string appUrl)
    {
        Uri uri = new Uri(appUrl);
        try
        {
            return uri.Segments[^1].Trim('/');
        }
        catch (Exception)
        {
            return null;
        }
    }

    private string? GetUserNameFromUrl(string friendUrl)
    {
        var clearedUrl = friendUrl.TrimEnd('/');
        var lastSlashIndex = clearedUrl.LastIndexOf('/');
        try
        {
            return clearedUrl[(lastSlashIndex + 1)..];
        }
        catch (Exception)
        {
            return null;
        }
    }
}