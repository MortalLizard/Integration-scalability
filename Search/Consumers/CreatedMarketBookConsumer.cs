using Elastic.Clients.Elasticsearch;
using Serilog;
using Shared;
using Shared.Contracts.CreateBook;

namespace Search.Consumers;

public sealed class CreatedMarketBookConsumer : BaseConsumer<MarketplaceBookCreated>
{
    protected override string QueueName => "marketplace.book-created";

    public CreatedMarketBookConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer)
    {
    }

    protected override async Task HandleMessageAsync(MarketplaceBookCreated command, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var elasticClient = serviceProvider.GetRequiredService<ElasticsearchClient>();

        try
        {
            var response = await elasticClient.IndexAsync(
                command,
                idx => idx
                    .Index("books")
                    .Id(command.Id),
                cancellationToken);

            if (!response.IsValidResponse)
                throw new InvalidOperationException($"Failed to index book {command.Id}: {response.DebugInformation}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error indexing book {BookId}", command.Id);
            throw;
        }
    }
}
