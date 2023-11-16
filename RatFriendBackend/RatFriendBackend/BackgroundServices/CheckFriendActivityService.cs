using Microsoft.EntityFrameworkCore;
using RatFriendBackend.HttpClients;
using RatFriendBackend.Persistence;
using Telegram.Bot;

namespace RatFriendBackend.BackgroundServices;

public class CheckFriendActivityService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISteamApiClient _steamApiClient;
    private const int TIME_BETWEEN_CHECK_FOR_NOTIFICATION_IN_MINUTES = 1;
    private readonly SemaphoreSlim _checkFriendsActivitySemaphore = new SemaphoreSlim(1, 1);
    private Timer _timer = null!;
    
    private readonly TelegramBotClient _telegramBotClient;

    public CheckFriendActivityService(IServiceProvider serviceProvider, ISteamApiClient steamApiClient)
    {
        _serviceProvider = serviceProvider;
        _steamApiClient = steamApiClient;
        _telegramBotClient = new TelegramBotClient(Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")!);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Create timer with 1 minute interval and doesn't start next tick until previous is finished
        _timer = new Timer(async _ => await CheckFriendsActivity(cancellationToken), null, TimeSpan.Zero,
            TimeSpan.FromMinutes(TIME_BETWEEN_CHECK_FOR_NOTIFICATION_IN_MINUTES));
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

        var friendsActivities = await context.FriendActivities
            .Include(x => x.FriendActivitySubscriptions)!
            .ThenInclude(x => x.UserSubscription)
            .GroupBy(x => x.FriendId)
            .ToListAsync(cancellationToken: stoppingToken);

        foreach (var friendActivities in friendsActivities)
        {
            var lastTimeUserActivity = friendActivities.ToDictionary(x => x.AppId, x => x);
            var recentlyPlayedGames =
                (await _steamApiClient.GetRecentlyPlayedGamesAsync(friendActivities.First().FriendId))
                .RecentlyPlayedGames.ToDictionary(x => x.AppId, x => x.PlaytimeForever);
            var gamesSubscribedTo =
                recentlyPlayedGames.IntersectBy(friendActivities.Select(x => x.AppId), x => x.Key);
            foreach (var game in gamesSubscribedTo)
            {
                if (recentlyPlayedGames[game.Key] > lastTimeUserActivity[game.Key].TimePlayed)
                {
                    // Смотрим по всем играм
                    var activityForNotification = friendActivities.First(x => x.AppId == game.Key);

                    var subscribers = activityForNotification.FriendActivitySubscriptions!
                        .Where(x => x.UserSubscription!.IsFollowing)
                        .ToList();
                    foreach (var subscriber in subscribers.Select(x => x.UserSubscription))
                    {
                        Console.WriteLine(
                            $"Твой друг: {activityForNotification.FriendName} ({activityForNotification.ProfileUrl}) играет в {activityForNotification.AppName} без тебя!\n" +
                            $"Пора оставить ему \"добрый\" комментарий!");
                        // Шлем сообщение в тг
                        try
                        {
                            await _telegramBotClient.SendTextMessageAsync(
                                subscriber.UserId,
                                $"Твой друг: {activityForNotification.FriendName} играет в {activityForNotification.AppName}!\n" +
                                $"Пора оставить ему \"добрый\" комментарий!", cancellationToken: stoppingToken);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Can't send message to {subscriber.UserId}");
                        }
                    }

                    activityForNotification.TimePlayed = recentlyPlayedGames[activityForNotification.AppId];
                }
            }

            await context.SaveChangesAsync(stoppingToken);
        }

        await context.SaveChangesAsync(stoppingToken);
        _checkFriendsActivitySemaphore.Release();
    }
}