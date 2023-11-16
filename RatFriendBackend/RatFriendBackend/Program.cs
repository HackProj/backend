using Microsoft.EntityFrameworkCore;
using RatFriendBackend;
using RatFriendBackend.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddApiLayer()
    .AddDalLayer(builder.Configuration)
    .AddBackgroundWorkers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await using (var db = scope.ServiceProvider.GetService<ApplicationDbContext>())
    {
        await db.Database.MigrateAsync();
    }
}

// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();