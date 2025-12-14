using Shared;
using Shared.Contracts.OrderBook.Shipping;

namespace Orchestrator.OrderSaga.Consumers;

public sealed class ShippingFailedConsumer : BaseConsumer<ShippingFailed>
{
    protected override string QueueName => "shipping.failed";

    public ShippingFailedConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer) { }

    protected override async Task HandleMessageAsync(ShippingFailed message, IServiceProvider sp, CancellationToken ct)
    {
        var pm = sp.GetRequiredService<IOrderProcessManager>();
        await pm.HandleShippingFailedAsync(message, ct);
    }
}
