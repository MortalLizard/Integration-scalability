using Inventory.Consumers;
using Inventory.Database.Services;
using Inventory.Database;
using Inventory.Logic;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Setup database connection
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("InventoryDatabase")));

// Register consumer
builder.Services.AddHostedService<OrderItemConsumer>();

// Register inventory logic
builder.Services.AddScoped<IOrderItemLogic, OrderItemLogic>();
builder.Services.AddScoped<IBookService, BookService>();

var host = builder.Build();

await host.RunAsync();
