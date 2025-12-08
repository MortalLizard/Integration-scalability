using System.Text.Json;
using Marketplace.Business.Interfaces;
using Marketplace.Contracts.Commands;
using Shared;

namespace Marketplace.Consumers;

public class OrderItemConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Consumer _consumer;
    private const string QueueName = "marketplace.order-item.process";

    public OrderItemConsumer(IServiceScopeFactory serviceScopeFactory, Consumer consumer)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.StartAsync(
            queueName: QueueName,
            handler: async (message, ct) =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var orderItemLogic = scope.ServiceProvider.GetRequiredService<IOrderItemLogic>();

                var dto = JsonSerializer.Deserialize<OrderItemProcess>(message)!;
                await orderItemLogic.ProcessOrderItem(dto, ct);
            },
            cancellationToken: stoppingToken
        );

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consumer.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
