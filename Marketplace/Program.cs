using Marketplace.Consumers;
using Marketplace.Business.Interfaces;
using Marketplace.Business.Services;
using Marketplace.Database.Repositories;
using Marketplace.Database.DBContext;
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
string connectionString = builder.Configuration.GetConnectionString("MarketplaceDatabase")
                          ?? "Server=marketplace-db;Database=marketplace;User Id=sa;Password=Marketplace@123;TrustServerCertificate=True;";

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

//create rabbitmq connection singleton and producer service
builder.Services.AddRabbitInfrastructure();
builder.Services.AddSingleton<Producer>();
builder.Services.AddTransient<Consumer>();

// Add consumer as hosted services
builder.Services.AddHostedService<OrderItemConsumer>();
builder.Services.AddHostedService<CreateBookConsumer>();

// Add services for dependency injection
builder.Services.AddScoped<ICreateBookLogic, CreateBookLogic>();
builder.Services.AddScoped<IOrderItemLogic, OrderItemLogic>();
builder.Services.AddScoped<IBookRepository, BookRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

