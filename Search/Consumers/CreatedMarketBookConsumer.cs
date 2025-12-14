using Serilog;

using Shared;
using Shared.Contracts.CreateBook;
namespace Search.Consumers;

public class CreatedMarketBookConsumer<TBook> : BaseConsumer<TBook>
{
    protected override string QueueName => "marketplace.book-created";
    public CreatedMarketBookConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer)
    {
    }

    protected override Task HandleMessageAsync(TBook command, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        return UpdateSearchIndex(command, serviceProvider, cancellationToken);
    }

    private async Task UpdateSearchIndex(TBook command, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var elasticClient = serviceProvider.GetRequiredService<Elastic.Clients.Elasticsearch.ElasticsearchClient>();

        try
        {
            var response = await elasticClient.IndexAsync<BookCreated>(
            command,
            idx => idx
                .Index("books")
                .Id(command.Id),
            cancellationToken);

            if (!response.IsValidResponse)
            {
                throw new InvalidOperationException($"Failed to index book {command.Id}: {response.DebugInformation}");
            }
        }catch(Exception ex)
        {
            Log.Error(ex, "Error indexing book {BookId}", command.Id);
            throw;

        }
   }
}
