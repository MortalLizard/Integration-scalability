using Elastic.Clients.Elasticsearch;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

//ELasticsearch client
builder.Services.AddSingleton(sp =>
{
    var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9201"))
        .DefaultIndex("books"); // optional default index

    return new ElasticsearchClient(settings);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapControllers();
app.UseHttpsRedirection();

app.Run();
