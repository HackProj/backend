using Microsoft.EntityFrameworkCore;
using RatFriendBackend.Persistence.Models;

namespace RatFriendBackend.Persistence;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<FriendActivityInfo> FriendActivityInfos { get; set; } = null!;
    public DbSet<UserFriendActivity> UserFriendActivities { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserFriendActivity>().HasKey(e => new { e.UserId, FriendGamesInfoId = e.FriendId });
    }
}