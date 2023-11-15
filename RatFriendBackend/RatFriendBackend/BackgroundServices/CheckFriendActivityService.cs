using Microsoft.EntityFrameworkCore;
using RatFriendBackend.HttpClients;
using RatFriendBackend.Persistence;
using RatFriendBackend.Persistence.Models;

namespace RatFriendBackend.BackgroundServices;

public class CheckFriendActivityService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISteamApiClient _steamApiClient;
    private const int TIME_BETWEEN_CHECK_FOR_NOTIFICATION_IN_MINUTES = 5;
    private readonly SemaphoreSlim _checkFriendsActivitySemaphore = new SemaphoreSlim(1, 1);

    //private readonly TelegramBotClient _telegramBotClient;
    private Timer _timer = null!;

    public CheckFriendActivityService(IServiceProvider serviceProvider, ISteamApiClient steamApiClient)
    {
        _serviceProvider = serviceProvider;
        _steamApiClient = steamApiClient;
        //        _telegramBotClient = new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")!);
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
        await _checkFriendsActivitySemaphore.WaitAsync(stoppingToken);
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var friendActivities = await context.FriendActivities
            .Include(x => x.FriendActivitySubscriptions)!
            .ThenInclude(x => x.UserSubscription)
            .GroupBy(x => x.FriendId)
            .ToListAsync(cancellationToken: stoppingToken);

        foreach (var friendActivity in friendActivities)
        {
            var lastTimeUserActivity = friendActivity.ToDictionary(x => x.AppId, x => x);
            var recentlyPlayedGames =
                (await _steamApiClient.GetRecentlyPlayedGamesAsync(friendActivity.First().FriendId))
                .RecentlyPlayedGames.ToDictionary(x => x.AppId, x => x.PlaytimeForever);
            var gamesSubscribedTo =
                recentlyPlayedGames.IntersectBy(friendActivity.Select(x => x.AppId), x => x.Key);
            foreach (var game in gamesSubscribedTo)
            {
                if (recentlyPlayedGames[game.Key] - lastTimeUserActivity[game.Key].TimePlayed >=
                    TIME_BETWEEN_CHECK_FOR_NOTIFICATION_IN_MINUTES)
                {
                    // Смотрим по всем играм
                    foreach (FriendActivity activity in friendActivity)
                    {
                        var subscribers = activity.FriendActivitySubscriptions!
                            .Where(x => x.UserSubscription!.IsFollowing)
                            .ToList();
                        foreach (var subscriber in subscribers.Select(x => x.UserSubscription))
                        {
                            Console.WriteLine(
                                $"Твой друг: {activity.FriendName} играет в {activity.AppName} без тебя!\n" +
                                $"Пора оставить ему \"добрый\" комментарий!");
                            // Шлем сообщение в тг
                            /*_telegramBotClient.SendTextMessageAsync(
                                    subscriber.UserId,
                                    $"Твой друг: {activity.FriendName} играет в {activity.AppName}!\n" +
                                    $"Пора оставить ему \"добрый\" комментарий!");
                                subscriber.UserId*/
                        }

                        activity.TimePlayed = recentlyPlayedGames[activity.AppId];
                    }
                }
            }

            await context.SaveChangesAsync(stoppingToken);
        }

        await context.SaveChangesAsync(stoppingToken);
        _checkFriendsActivitySemaphore.Release();
    }
}