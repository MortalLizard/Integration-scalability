using Elastic.Clients.Elasticsearch;
using Search.Consumers;
using Search.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

//ELasticsearch client
builder.Services.AddElasticsearch();
builder.Services.AddScoped<BookSearchSeeder>();

// Hosted Services
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
