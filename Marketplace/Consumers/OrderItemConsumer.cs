using System.Text.Json;
using Marketplace.Business.Interfaces;
using Marketplace.Contracts.Commands;

using Serilog;

using Shared;

namespace Marketplace.Consumers;

public class OrderItemConsumer(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private Consumer? consumer;
    private const string queueName = "marketplace.order-item.process";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        consumer = await Consumer.CreateAsync(
            queueName: queueName,
            handler: async (message, ct) =>
            {
                Log.Error("Received message: {Message}", message ?? "null");
                using var scope = serviceScopeFactory.CreateScope();
                var orderItemLogic = scope.ServiceProvider.GetRequiredService<IOrderItemLogic>();

                var dto = JsonSerializer.Deserialize<OrderItemProcess>(message)!;
                await orderItemLogic.ProcessOrderItem(dto, ct);
            },
            cancellationToken: stoppingToken
        );
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (consumer is not null)
        {
            await consumer.DisposeAsync();
            consumer = null;
        }

        await base.StopAsync(cancellationToken);
    }
}
