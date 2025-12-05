using Inventory.Consumers;
using Inventory.Database.Services;
using Inventory.Database;
using Inventory.Logic;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Setup database connection
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("InventoryDatabase") ?? "Server=inventory-db;Database=inventory;User Id=sa;Password=Inventory@123;TrustServerCertificate=True;"));

// Add MVC services
builder.Services.AddControllersWithViews();

// Register consumer as a hosted service
builder.Services.AddHostedService<OrderItemConsumer>();

// Register Producer as a Singleton so the connection is shared
builder.Services.AddSingleton<Shared.Producer>();

// Register inventory logic
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
