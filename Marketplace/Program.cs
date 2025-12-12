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

// Add controllers
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddOpenApi();

// Setup db
string connectionString = builder.Configuration.GetConnectionString("MarketplaceDatabase")
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

//create rabbitmq connection singleton and producer service
builder.Services.AddRabbitInfrastructure();
builder.Services.AddSingleton<Producer>();
builder.Services.AddTransient<IConsumer, Consumer>();

// Add consumer as hosted services
builder.Services.AddHostedService<OrderlineConsumer>();
builder.Services.AddHostedService<CreateBookConsumer>();

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
app.MapControllers();
app.UseHttpsRedirection();

app.Run();

