using Marketplace.Consumers;
using Marketplace.Business.Interfaces;
using Marketplace.Business.Services;
using Marketplace.Database.Repositories;
using Marketplace.Database.DBContext;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Serilog;

using Shared;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog as the default static logger
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSerilog();

// Add services to the container.
builder.Services.AddOpenApi();

// Setup db
string connectionString = builder.Configuration.GetConnectionString("Default")
                          ?? "Server=shared-db;Database=MarketplaceDb;User Id=sa;Password=Shared@123!;TrustServerCertificate=True;";

builder.Services.AddDbContextPool<MarketplaceDbContext>(options =>
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

// Setup rabbitmq connection
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderlineConsumer>();
    x.AddConsumer<CreateBookConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("rabbit");
            h.Password("rabbit_pw");
        });

        cfg.ReceiveEndpoint("marketplace-orderline-process", e =>
        {
            e.ConfigureConsumer<OrderlineConsumer>(context);
        });
        cfg.ReceiveEndpoint("marketplace-book-create", e =>
        {
            e.ConfigureConsumer<CreateBookConsumer>(context);
        });
    });
});

// Add services for dependency injection
builder.Services.AddScoped<ICreateBookLogic, CreateBookLogic>();
builder.Services.AddScoped<IOrderlineLogic, OrderlineLogic>();
builder.Services.AddScoped<IBookRepository, BookRepository>();

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
        var db = services.GetRequiredService<MarketplaceDbContext>();
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

app.Run();

