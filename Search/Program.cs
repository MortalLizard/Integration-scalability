using Elastic.Clients.Elasticsearch;

using Search.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

//ELasticsearch client
builder.Services.AddElasticsearch();
builder.Services.AddScoped<BookSearchSeeder>();

var app = builder.Build();

// Seed data on startup (optional: only in Development)
using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    if (env.IsDevelopment())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<BookSearchSeeder>();
        await seeder.SeedAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapControllers();
app.UseHttpsRedirection();

app.Run();
