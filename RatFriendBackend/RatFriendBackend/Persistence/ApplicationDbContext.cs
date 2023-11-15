using Microsoft.EntityFrameworkCore;
using RatFriendBackend.Persistence.Models;

namespace RatFriendBackend.Persistence;

public class ApplicationDbContext : DbContext
{
    public DbSet<UserSubscription> UserSubscription { get; set; } = null!;
    public DbSet<FriendActivitySubscription> FriendActivitySubscriptions { get; set; } = null!;
    public DbSet<FriendActivity> FriendActivities { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSubscription>().HasIndex(e => new { e.UserId, e.FriendId }).IsUnique();
        modelBuilder.Entity<FriendActivity>().HasIndex(e => new { e.FriendId, e.AppId, }).IsUnique();
    }
}