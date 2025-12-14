using Shared;
using Shared.Contracts.OrderBook;

namespace Orchestrator.OrderSaga.Consumers;

public sealed class InventoryOrderlineProcessFailedConsumer : BaseConsumer<InventoryOrderlineProcessFailed>
{
    protected override string QueueName => "inventory.order-item.process.failed";

    public InventoryOrderlineProcessFailedConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer) { }

    protected override async Task HandleMessageAsync(InventoryOrderlineProcessFailed message, IServiceProvider sp, CancellationToken ct)
    {
        var pm = sp.GetRequiredService<IOrderProcessManager>();
        await pm.HandleInventoryLineFailedAsync(message, ct);
    }
}
