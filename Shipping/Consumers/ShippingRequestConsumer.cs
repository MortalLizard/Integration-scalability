using Serilog;
using Shared;
using Shared.Contracts.OrderBook.Shipping;

namespace Shipping.Consumers;

public sealed class ShippingRequestConsumer : BaseConsumer<ShippingRequest>
{
    protected override string QueueName => "shipping.request";

    public ShippingRequestConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer)
    {
    }

    protected override async Task HandleMessageAsync(
        ShippingRequest command,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(120), cancellationToken);

        var success = true;
        var producer = serviceProvider.GetRequiredService<Producer>();

        if (success)
        {
            Log.Information("Shipping stub completed. OrderId={OrderId}", command.CorrelationId);
            await producer.SendMessageAsync("shipping.completed", new ShippingCompleted(command.CorrelationId));
        }
        else
        {
            Log.Warning("Shipping stub failed. OrderId={OrderId}", command.CorrelationId);
            await producer.SendMessageAsync("shipping.failed", new ShippingFailed(command.CorrelationId, "Shipping failed (stub)"));
        }
    }
}
