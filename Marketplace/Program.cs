using Inventory.Consumers;

using Marketplace.Business.Interfaces;
using Marketplace.Business.Services;
using Marketplace.Data.Repositories;
using Marketplace.Data.Repositories.DBContext;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
                sqlOptions.CommandTimeout(60);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null
                );
            })
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors()
);

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

