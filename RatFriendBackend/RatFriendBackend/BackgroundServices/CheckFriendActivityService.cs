using Microsoft.EntityFrameworkCore;
using RatFriendBackend.HttpClients;
using RatFriendBackend.Persistence;
using RatFriendBackend.Persistence.Models;
using SteamWebAPI2.Utilities;
using Telegram.Bot;

namespace RatFriendBackend.BackgroundServices;

public class CheckFriendActivityService : IHostedService
{
    private readonly ISteamApiRefit _steamApiClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly TelegramBotClient _telegramBotClient;
    private Timer _timer = null!;

    public CheckFriendActivityService(ISteamApiRefit steamApiClient, IServiceProvider serviceProvider)
    {
        _steamApiClient = steamApiClient;
        _serviceProvider = serviceProvider;
        _telegramBotClient = new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")!);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Create timer with 1 minute interval and doesn't start next tick until previous is finished
        _timer = new Timer(async _ => await CheckFriendsActivity(cancellationToken), null, TimeSpan.Zero,
            TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _timer.DisposeAsync();
    }

    private async Task CheckFriendsActivity(CancellationToken stoppingToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var chunkSize = 100_000;
            var subscriptionsCount =
                await context.UserFriendActivities.LongCountAsync(cancellationToken: stoppingToken);
            for (int i = 0; i < subscriptionsCount / chunkSize; i++)
            {
                var subscriptions = await context.UserFriendActivities
                    .Include(x => x.User)
                    .Include(x => x.FriendActivityInfo)
                    .Skip(i * chunkSize)
                    .Take(chunkSize)
                    .DistinctBy(x => x.User!.ApiToken)
                    .ToListAsync(cancellationToken: stoppingToken);

                await Parallel.ForEachAsync(subscriptions, stoppingToken,
                    async (userFriendActivity, _) => { await ProcessCurrentFriendActivity(userFriendActivity); });

                await context.SaveChangesAsync(cancellationToken: stoppingToken);
            }
        }
    }

    private async Task ProcessCurrentFriendActivity(UserFriendActivity userFriendActivity)
    {
        var curGamesInfo = (await _steamApiClient.GetRecentlyPlayedGamesAsync(
                userFriendActivity.User!.ApiToken,
                userFriendActivity.FriendId)).RecentlyPlayedGamesResponse.Games
            .Select(g => new GamesInfo(g.name, g.appid, g.playtime_forever))
            .ToList();
        var prevGamesInfo = userFriendActivity.FriendActivityInfo!.GameInfos;
        var gameThatFriendPlayedWithoutUs = GetGameNameThatUserPlayNow(prevGamesInfo, curGamesInfo);
        var friendPlayWithoutUs = gameThatFriendPlayedWithoutUs == null;
        var friendStillPlayingWithoutUs = userFriendActivity.FriendActivityInfo.StillPlayingWithoutUs;

        if (friendPlayWithoutUs)
        {
            userFriendActivity.FriendActivityInfo.LastPlayTime = DateTime.UtcNow.ToUnixTimeStamp();
        }

        if (friendPlayWithoutUs && !friendStillPlayingWithoutUs)
        {
            userFriendActivity.FriendActivityInfo.StillPlayingWithoutUs = true;
            await _telegramBotClient.SendTextMessageAsync(
                userFriendActivity.User.TelegramId,
                "Ваш друг играет в " + gameThatFriendPlayedWithoutUs + "!"
            );
        }
        else if (!friendPlayWithoutUs && friendStillPlayingWithoutUs)
        {
            userFriendActivity.FriendActivityInfo.StillPlayingWithoutUs = false;
        }

        userFriendActivity.FriendActivityInfo.GameInfos = curGamesInfo;
    }

    private string? GetGameNameThatUserPlayNow(List<GamesInfo> prevGamesInfos, List<GamesInfo> curGamesInfos)
    {
        var newGames = curGamesInfos.ExceptBy(prevGamesInfos.Select(x => x.GameId), x => x.GameId).ToList();
        if (newGames.Any())
            return newGames.First().Name;
        
        var sameGames = curGamesInfos.IntersectBy(prevGamesInfos.Select(x => x.GameId), x => x.GameId);
        var prevGamesInfosDict = prevGamesInfos.ToDictionary(x => x.GameId, x => x);
        foreach (var curGameInfo in sameGames)
        {
            var prevGameInfo = prevGamesInfosDict[curGameInfo.GameId];

            if (curGameInfo.PlayTimeForever > prevGameInfo.PlayTimeForever)
                return curGameInfo.Name;
        }

        return null;
    }
}