using Shared;
using Shared.Contracts.OrderBook;

namespace Orchestrator.OrderSaga.Consumers;

public sealed class MarketplaceOrderlineProcessFailedConsumer : BaseConsumer<MarketplaceOrderlineProcessFailed>
{
    protected override string QueueName => "marketplace.order-item.process.failed";

    public MarketplaceOrderlineProcessFailedConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer) { }

    protected override async Task HandleMessageAsync(MarketplaceOrderlineProcessFailed message, IServiceProvider sp, CancellationToken ct)
    {
        var pm = sp.GetRequiredService<IOrderProcessManager>();
        await pm.HandleMarketplaceLineFailedAsync(message, ct);
    }
}
