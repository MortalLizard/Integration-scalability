using Elastic.Clients.Elasticsearch;

namespace Search.Infrastructure;

public static class ElasticsearchServiceExtension
{
    public static IServiceCollection AddElasticsearch(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var settings = new ElasticsearchClientSettings(new Uri("http://elasticsearch:9200"))
                .DefaultIndex("books")
                .EnableHttpCompression();

            return new ElasticsearchClient(settings);
        });

        return services;
    }
}
