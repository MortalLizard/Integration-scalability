using Orchestrator.OrderSaga;
using Shared;
using Shared.Contracts.OrderBook.Shipping;

namespace Orchestrator.OrderSaga.Consumers;

public sealed class ShippingCompletedConsumer : BaseConsumer<ShippingCompleted>
{
    protected override string QueueName => "shipping.completed";

    public ShippingCompletedConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer) { }

    protected override async Task HandleMessageAsync(ShippingCompleted message, IServiceProvider sp, CancellationToken ct)
    {
        var pm = sp.GetRequiredService<IOrderProcessManager>();
        await pm.HandleShippingCompletedAsync(message, ct);
    }
}
