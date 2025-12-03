using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(new JsonFormatter()) // JSON for Filebeat / Filebeat-friendly
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// wire Serilog into the generic host
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

// Optional: Serilog middleware to log requests automatically
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/", (ILogger<Program> logger) =>
    {
        logger.LogInformation("Received request for weather forecast");

        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();

        logger.LogInformation("Returning {Count} forecast entries", forecast.Length);

        // example of logging a warning and a debug message
        logger.LogWarning("This is a sample warning for testing Filebeat ingestion");
        logger.LogDebug("Sample debug: first summary = {Summary}", forecast.FirstOrDefault()?.Summary);

        return forecast;
    })
    .WithName("GetWeatherForecast");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
