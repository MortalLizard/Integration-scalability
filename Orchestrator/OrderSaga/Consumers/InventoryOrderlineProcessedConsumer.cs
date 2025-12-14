using Shared;
using Shared.Contracts.OrderBook;

namespace Orchestrator.OrderSaga.Consumers;

public sealed class InventoryOrderlineProcessedConsumer : BaseConsumer<InventoryOrderlineProcessed>
{
    protected override string QueueName => "inventory.order-item.processed";

    public InventoryOrderlineProcessedConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer) { }

    protected override async Task HandleMessageAsync(InventoryOrderlineProcessed message, IServiceProvider sp, CancellationToken ct)
    {
        var pm = sp.GetRequiredService<IOrderProcessManager>();
        await pm.HandleInventoryLineSucceededAsync(message, ct);
    }
}
