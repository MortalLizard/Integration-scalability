using Serilog;
using Shared;
using Shared.Contracts.OrderBook.Shipping;

namespace Orchestrator.OrderSaga.Activities;

public sealed class ShippingActivity(Producer producer) : IShippingActivity
{
    public async Task ExecuteAsync(Guid orderId, CancellationToken ct = default)
    {
        await producer.SendMessageAsync("shipping.request", new ShippingRequest(orderId));
        Log.Information("Dispatched shipping.request OrderId={OrderId}", orderId);
    }

    public Task CompensateAsync(Guid orderId, CancellationToken ct = default)
    {
        Log.Warning("Compensate shipping. OrderId={OrderId}", orderId);
        return Task.CompletedTask;
    }
}
