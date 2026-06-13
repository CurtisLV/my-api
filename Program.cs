using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    // Added abortConnect=false so it does not crash on startup if offline
    var connectionString = builder.Configuration["REDIS_CONNECTION"] ?? "redis:6379,abortConnect=false";
    try
    {
        return ConnectionMultiplexer.Connect(connectionString);
    }
    catch
    {
        return null!; // Handle null gracefully in endpoints
    }
});

var app = builder.Build();

// Only redirect in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// ==================== Endpoints ====================

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Main endpoint - increments visit counter
app.MapGet("/weatherforecast", async (IConnectionMultiplexer redis) =>
{
    long totalVisits = 0;

    try
    {
        // Only try to use Redis if it is initialized and connected
        if (redis != null && redis.IsConnected)
        {
            var db = redis.GetDatabase();
            totalVisits = await db.StringIncrementAsync("visits:total");
        }
    }
    catch
    {
        // Redis is down; ignore the error and keep totalVisits at 0
    }

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return new
    {
        forecast,
        totalVisits
    };
});

// Just returns current count (does NOT increment)
app.MapGet("/visits", async (IConnectionMultiplexer redis) =>
{
    long count = 0;

    try
    {
        if (redis != null && redis.IsConnected)
        {
            var db = redis.GetDatabase();
            var totalVisits = await db.StringGetAsync("visits:total");
            count = totalVisits.HasValue ? (long)totalVisits : 0;
        }
    }
    catch
    {
        // Ignore
    }

    return Results.Ok(new
    {
        totalVisits = count
    });
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}