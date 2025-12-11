using Serilog;
using Shared;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog as the default static logger
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSerilog();

//create rabbitmq connection singleton and producer service
builder.Services.AddRabbitInfrastructure();
builder.Services.AddSingleton<Producer>();

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();


app.Run();

