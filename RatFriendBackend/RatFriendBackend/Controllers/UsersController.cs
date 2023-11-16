using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RatFriendBackend.HttpClients;
using RatFriendBackend.Persistence;
using RatFriendBackend.Persistence.Models;
using RatFriendBackend.Requests;

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

    [HttpGet("subscriptions")]
    public async Task<IActionResult> GetUserSubscriptions(long userId)
    {
        var userSubscriptions = await _context.FriendActivitySubscriptions
            .Include(x => x.UserSubscription)
            .Include(x => x.FriendActivity)
            .Where(x => x.UserSubscription!.UserId == userId)
            .Select(x => x.FriendActivity)
            .ToListAsync();

        if (userSubscriptions.Count <= 0)
            return NotFound("User with specified id not found");

        return Ok(userSubscriptions.DistinctBy(x => x!.FriendId)
            .Select(x => new
                {
                    x!.FriendName,
                    x.ProfileUrl
                }
            ));
    }

    [HttpGet("subscriptions/friend")]
    public async Task<IActionResult> GetUserSubscriptions(long userId, string friendUrl)
    {
        var (friendId, errorMsg) = await GetUserIdByUrl(friendUrl);
        if (friendId == null)
            return BadRequest(errorMsg);

        var userSubscriptions = await _context.FriendActivitySubscriptions
            .Include(x => x.UserSubscription)
            .Include(x => x.FriendActivity)
            .Where(x => x.UserSubscription!.UserId == userId && x.FriendActivity!.FriendId == friendId)
            .Select(x => x.FriendActivity)
            .ToListAsync();

        if (userSubscriptions.Count <= 0)
            return NotFound("User with specified id not found");

        return Ok(userSubscriptions.Select(x => new
                {
                    x!.FriendName,
                    x.ProfileUrl,
                    x.AppName,
                }
            ));
    }

    [HttpDelete("subscription/friend")]
    public async Task<IActionResult> DeleteUserSubscription(RemoveAllSubscriptionWithFriendRequest request)
    {
        var (friendId, errorMsg) = await GetUserIdByUrl(request.FriendUrl);
        if (friendId == null)
            return BadRequest(errorMsg);

        var userSubscriptions = await _context.FriendActivitySubscriptions
            .Include(x => x.UserSubscription)
            .Include(x => x.FriendActivity)
            .Where(x =>
                x.UserSubscription!.UserId == request.UserId
                && x.FriendActivity!.FriendId == friendId)
            .ToListAsync();

        _context.FriendActivitySubscriptions.RemoveRange(userSubscriptions);
        _context.UserSubscription.RemoveRange(userSubscriptions.Select(x => x.UserSubscription)!);
        _context.FriendActivities.RemoveRange(userSubscriptions.Select(x => x.FriendActivity)!);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("subscription/game")]
    public async Task<IActionResult> DeleteUserSubscription(RemoveSubscriptionRequest request)
    {
        var (friendId, errorMsg) = await GetUserIdByUrl(request.FriendUrl);
        if (friendId == null)
            return BadRequest(errorMsg);

        var userSubscriptions = await _context.FriendActivitySubscriptions
            .Include(x => x.UserSubscription)
            .Include(x => x.FriendActivity)
            .Where(x =>
                x.UserSubscription!.UserId == request.UserId
                && x.FriendActivity!.FriendId == friendId
                && x.FriendActivity.AppName == request.AppName)
            .ToListAsync();

        _context.FriendActivities.RemoveRange(userSubscriptions.Select(x => x.FriendActivity)!);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("subscribeStatus")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> ChangeSubscribeStatus(DisableNotificationRequest request)
    {
        var (friendId, errorMsg) = await GetUserIdByUrl(request.FriendUrl);
        if (friendId == null)
            return BadRequest(errorMsg);
        
        await CreateUserSubscriptionIfNotExists(friendId.Value, request.TelegramId);

        var userSubscription = await _context.UserSubscription
            .FirstOrDefaultAsync(x => x.UserId == request.TelegramId && x.FriendId == friendId);

        if (userSubscription == null)
            return NotFound("User is not subscribed to this friend");

        userSubscription.IsFollowing = request.IsEnabled;
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("subscribe")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> Subscribe(SubscribeToUserRequest request)
    {
        var (friendId, errorMsg) = await GetUserIdByUrl(request.FriendUrl);
        if (friendId == null)
            return BadRequest(errorMsg);
        await CreateUserSubscriptionIfNotExists(friendId.Value, request.TelegramId);

        var appName = GetAppNameFromUrl(request.AppUrl);
        if (appName == null)
            return BadRequest("Invalid app url");

        var appId = GetAppIdFromUrl(request.AppUrl);
        if (appId == null)
            return BadRequest("Invalid app url");

        errorMsg = await CreateFriendActivitiesIfNotExists(appId.Value, appName, friendId.Value);
        if (errorMsg != null)
            return BadRequest(errorMsg);

        await _context.SaveChangesAsync();
        await CreateFriendActivitySubscriptionIfNotExists(friendId.Value, request.TelegramId, appId.Value);
        await _context.SaveChangesAsync();

        return Ok();
    }

    private async Task<(ulong? userId, string msg)> GetUserIdByUrl(string friendUrl)
    {
        var userName = GetUserNameFromUrl(friendUrl);
        if (userName == null)
            return (null, "Invalid friend url");

        return (await _steamApiClient.ResolveVanityUrlAsync(userName), string.Empty);
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

        var app = (await _steamApiClient.GetOwnedGamesAsync(friendId)).OwnedGames
            .FirstOrDefault(x => x.AppId == appId);
        if (app == null)
            return "User doesn't own this game";

        var friend = (await _steamApiClient.GetPlayerSummariesAsync(friendId)).First();
        _context.FriendActivities.Add(new FriendActivity
        {
            FriendId = friendId,
            FriendName = friend.Nickname,
            ProfileUrl = friend.ProfileUrl,
            AppName = appName,
            AppId = appId,
            TimePlayed = (uint)app.PlaytimeForever.TotalMinutes
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
        if (!Uri.IsWellFormedUriString(friendUrl, UriKind.Absolute))
            return null;

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