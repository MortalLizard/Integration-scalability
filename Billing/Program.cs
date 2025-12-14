using Billing.Consumers;

using Shared;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

//create rabbitmq connection singleton and producer service
builder.Services.AddRabbitInfrastructure();
builder.Services.AddSingleton<Producer>();
builder.Services.AddTransient<IConsumer, Consumer>();

// Add consumer as hosted services
builder.Services.AddHostedService<BillingAuthorizeConsumer>();
builder.Services.AddHostedService<BillingInvoiceConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () =>
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
