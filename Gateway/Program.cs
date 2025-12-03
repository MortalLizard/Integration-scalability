using Shared;

var builder = WebApplication.CreateBuilder(args);

var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
var rabbitUser = Environment.GetEnvironmentVariable("RABBITMQ_USER");
var rabbitPass = Environment.GetEnvironmentVariable("RABBITMQ_PASS");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapGet("/new", async () =>
    {
        var producer = new Producer("localhost:5672", "test");
        await producer.SendMessageAsync("Test");
        return Results.Ok("Message sent");

    });

app.Run();

