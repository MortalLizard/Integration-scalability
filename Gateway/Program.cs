using MassTransit;
using Serilog;
using Shared;
using Shared.Contracts.CreateBook;
using Shared.Contracts.OrderBook;
using Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog as the default static logger
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Services.AddSerilog();

//create rabbitmq connection singleton and producer service
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context,cfg) =>
    {
        cfg.Host("rabbitmq", "/", h => {
            h.Username("rabbit");
            h.Password("rabbit_pw");
        });
    });
});

EndpointConvention.Map<InventoryOrderlineProcess>(
    new Uri("queue:inventory-orderline-process"));

EndpointConvention.Map<MarketplaceOrderlineProcess>(
    new Uri("queue:marketplace-orderline-process"));

EndpointConvention.Map<BookCreate>(
    new Uri("queue:marketplace-book-create"));

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

