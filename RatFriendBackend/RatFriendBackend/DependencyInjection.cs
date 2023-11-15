using Microsoft.EntityFrameworkCore;
using RatFriendBackend.BackgroundServices;
using RatFriendBackend.HttpClients;
using RatFriendBackend.Persistence;
using Refit;

namespace RatFriendBackend;

public static class DependencyInjection
{
    public static IServiceCollection AddApiLayer(this IServiceCollection services)
    {
        services
            .AddRefitClient<ISteamApiRefit>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.steampowered.com"));
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.RegisterServices();
        return services;
    }

    private static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton<ISteamApiClient, SteamApiClient>();
        return services;
    }

    private static IServiceCollection RegisterHangfireTasks(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddDalLayer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["ConnectionStrings:Default"];
        services.AddDbContext<ApplicationDbContext>(options => { options.UseNpgsql(connectionString); });

        return services;
    }

    public static IServiceCollection AddBackgroundWorkers(this IServiceCollection services)
    {
        services.AddHostedService<CheckFriendActivityService>();
        return services;
    }
}