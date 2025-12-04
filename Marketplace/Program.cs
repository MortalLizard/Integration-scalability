using Marketplace.Data.Repositories.DBContext;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddDbContext<MarketplaceDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MarketplaceDatabase")));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

