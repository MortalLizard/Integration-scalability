using Inventory.Consumers;
using Inventory.Database.Services;
using Inventory.Database;
using Inventory.Logic;
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

// Setup database connection
string connectionString = builder.Configuration.GetConnectionString("InventoryDatabase")
                          ?? "Server=inventory-db;Database=inventory;User Id=sa;Password=Inventory@123;TrustServerCertificate=True;";

builder.Services.AddDbContextPool<InventoryDbContext>(options =>
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

// Add MVC services
builder.Services.AddControllersWithViews();

// Register consumer as a hosted service
builder.Services.AddHostedService<OrderItemConsumer>();

// Register inventory logicBookRepository
builder.Services.AddScoped<IOrderItemLogic, OrderItemLogic>();
builder.Services.AddScoped<IBookService, BookService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Book}/{action=Index}/{id?}");

await app.RunAsync();
