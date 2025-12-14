using Shared;
using Shared.Contracts.OrderBook;

namespace Orchestrator.OrderSaga.Consumers;

public sealed class MarketplaceOrderlineProcessedConsumer : BaseConsumer<MarketplaceOrderlineProcessed>
{
    protected override string QueueName => "marketplace.order-item.processed";

    public MarketplaceOrderlineProcessedConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer) { }

    protected override async Task HandleMessageAsync(MarketplaceOrderlineProcessed message, IServiceProvider sp, CancellationToken ct)
    {
        var pm = sp.GetRequiredService<IOrderProcessManager>();
        await pm.HandleMarketplaceLineSucceededAsync(message, ct);
    }
}
