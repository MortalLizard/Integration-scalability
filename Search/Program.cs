using Elastic.Clients.Elasticsearch;
using Search.Consumers;
using Search.Infrastructure;

using Serilog;

using Shared;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Configure Serilog as the default static logger
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSerilog();

//ELasticsearch client
builder.Services.AddElasticsearch();
builder.Services.AddScoped<BookSearchSeeder>();

// Hosted Services
builder.Services.AddRabbitInfrastructure();
builder.Services.AddTransient<IConsumer, Consumer>();

builder.Services.AddHostedService<CreatedMarketBookConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapControllers();
app.UseHttpsRedirection();

app.Run();
