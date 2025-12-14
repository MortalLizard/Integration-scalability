using Microsoft.EntityFrameworkCore;
using Orchestrator.OrderSaga;
using Orchestrator.OrderSaga.Activities;
using Orchestrator.OrderSaga.Consumers;
using Orchestrator.OrderSaga.Database.DbContext;
using Orchestrator.OrderSaga.Database.Repository;
using Serilog;
using Shared;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog as the default static logger
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSerilog();

// Setup db
string connectionString = builder.Configuration.GetConnectionString("Default")
                          ?? "Server=localhost,14333;Database=OrderDb;User Id=sa;Password=Shared@123!;TrustServerCertificate=True;";

builder.Services.AddDbContextPool<OrderDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null
            );
        })
);

//create rabbitmq connection singleton and producer service
builder.Services.AddRabbitInfrastructure();
builder.Services.AddSingleton<Producer>();
builder.Services.AddTransient<IConsumer, Consumer>();

// Services used in OrderSaga
builder.Services.AddScoped<IInventoryOrderLineActivity, InventoryOrderLineActivity>();
builder.Services.AddScoped<IMarketplaceOrderLineActivity, MarketplaceOrderLineActivity>();
builder.Services.AddScoped<IBillingAuthorizeActivity, BillingAuthorizeActivity>();
builder.Services.AddScoped<IBillingInvoiceActivity, BillingInvoiceActivity>();
builder.Services.AddScoped<IShippingActivity, ShippingActivity>();
builder.Services.AddScoped<ISearchUpdateActivity, SearchUpdateActivity>();

builder.Services.AddScoped<IOrderProcessManager, OrderProcessManager>();
builder.Services.AddScoped<IOrderSagaRepository, OrderSagaRepository>();

builder.Services.AddHostedService<InventoryOrderlineProcessedConsumer>();
builder.Services.AddHostedService<InventoryOrderlineProcessFailedConsumer>();
builder.Services.AddHostedService<MarketplaceOrderlineProcessedConsumer>();
builder.Services.AddHostedService<MarketplaceOrderlineProcessFailedConsumer>();
builder.Services.AddHostedService<ShippingCompletedConsumer>();
builder.Services.AddHostedService<ShippingFailedConsumer>();
builder.Services.AddHostedService<BillingInvoicedConsumer>();
builder.Services.AddHostedService<BillingInvoiceFailedConsumer>();
builder.Services.AddHostedService<BillingAuthorizedConsumer>();
builder.Services.AddHostedService<BillingAuthorizationFailedConsumer>();

builder.Services.AddOpenApi();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<OrderDbContext>();
        db.Database.Migrate();
        Log.Information("Database migrations applied.");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An error occurred while applying database migrations.");
        throw;
    }
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

