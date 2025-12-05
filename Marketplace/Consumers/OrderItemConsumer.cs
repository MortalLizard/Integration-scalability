using System.Text.Json;
using Marketplace.Business.Interfaces;
using Marketplace.Contracts.Commands;

using Shared;

namespace Inventory.Consumers;

public class OrderItemConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private Consumer? _consumer;
    private readonly string _queueName = "marketplace.order-items";

    public OrderItemConsumer(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer = await Consumer.CreateAsync(
            queueName: _queueName,
            handler: async (message, ct) =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var orderItemLogic = scope.ServiceProvider.GetRequiredService<IOrderItemLogic>();

                    var dto = JsonSerializer.Deserialize<OrderItemProcess>(message)!;
                    await orderItemLogic.ProcessOrderItem(dto, ct);
                }
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
        if (_consumer is not null)
        {
            await _consumer.DisposeAsync();
            _consumer = null;
        }

        await base.StopAsync(cancellationToken);
    }
}
