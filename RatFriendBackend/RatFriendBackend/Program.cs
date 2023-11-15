using RatFriendBackend;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddApiLayer()
    .AddDalLayer(builder.Configuration)
    .AddBackgroundWorkers();

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();