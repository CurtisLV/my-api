using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration["REDIS_CONNECTION"] ?? "redis:6379";
    return ConnectionMultiplexer.Connect(connectionString);
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
    var db = redis.GetDatabase();

    // Increment counter every time this endpoint is called
    var totalVisits = await db.StringIncrementAsync("visits:total");

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
        totalVisits   // optional: you can return the count too
    };
});

// Just returns current count (does NOT increment)
app.MapGet("/visits", async (IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var totalVisits = await db.StringGetAsync("visits:total");

    long count = totalVisits.HasValue ? (long)totalVisits : 0;
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