using Shared;
using Shared.Infrastructure;

using Shipping.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

//create rabbitmq connection singleton and producer service
builder.Services.AddRabbitInfrastructure();
builder.Services.AddSingleton<Producer>();
builder.Services.AddTransient<IConsumer, Consumer>();

// Add consumer as hosted services
builder.Services.AddHostedService<ShippingRequestConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/health", () =>
    {
        var healthInfo = new HealthInfo(
            Status: "Healthy",
            Timestamp: DateTime.UtcNow,
            Application: "Search",
            Version: "1.0.0"
        );

        return TypedResults.Ok(healthInfo);
    })
    .WithName("Health");

app.Run();

public sealed record HealthInfo(
    string Status,
    DateTime Timestamp,
    string Application,
    string Version
);
