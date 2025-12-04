using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Shared;
using Inventory.DTOs;
using Inventory.Logic;

namespace Inventory.Consumers;

public class OrderItemConsumer : BackgroundService
{
    private readonly IOrderItemLogic _orderItemLogic;
    private Consumer? _consumer;
    private readonly string _queueName = "inventory.order-items";

    public OrderItemConsumer(IOrderItemLogic orderItemLogic)
    {
        _orderItemLogic = orderItemLogic;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer = await Consumer.CreateAsync(
            queueName: _queueName,
            handler: async (message, ct) =>
            {
                var dto = JsonSerializer.Deserialize<OrderItemDto>(message)!;
                await _orderItemLogic.ProcessOrderItem(dto, ct);
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
