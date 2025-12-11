using Microsoft.EntityFrameworkCore;

using Orchestrator.OrderSaga;
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
                          ?? "Server=shared-db;Database=OrderDb;User Id=sa;Password=Shared@123!;TrustServerCertificate=True;";

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
builder.Services.AddTransient<Consumer>();

builder.Services.AddScoped<IOrderProcessManager, OrderProcessManager>();
builder.Services.AddScoped<IOrderSagaRepository, OrderSagaRepository>();

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

