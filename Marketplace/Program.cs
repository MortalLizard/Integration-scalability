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
builder.Services.AddDbContext<MarketplaceDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MarketplaceDatabase")));

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

