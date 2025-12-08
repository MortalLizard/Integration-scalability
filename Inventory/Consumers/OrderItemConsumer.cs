using System.Text.Json;
using Inventory.Contracts.Commands;
using Inventory.Logic;
using Shared;

namespace Inventory.Consumers;

public class OrderItemConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Consumer _consumer;
    private const string queueName = "inventory.order-item.process";

    public OrderItemConsumer(IServiceScopeFactory serviceScopeFactory, Consumer consumer)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.StartAsync(
            queueName: queueName,
            handler: async (message, ct) =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var orderItemLogic = scope.ServiceProvider.GetRequiredService<IOrderItemLogic>();

                var dto = JsonSerializer.Deserialize<OrderItemProcess>(message)!;
                await orderItemLogic.ProcessOrderItem(dto, ct);
            },
            cancellationToken: stoppingToken);

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
        // Dispose the underlying RabbitMQ channel/consumer
        await _consumer.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }
}
