using Shared;
using Inventory.Logic;

namespace Inventory.Consumers;

public class OrderItemConsumer(Consumer consumer, IOrderItemLogic orderItemLogic) : BackgroundService
{
    private readonly Consumer consumer = consumer;
    private readonly IOrderItemLogic orderItemLogic = orderItemLogic;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Inventory Worker started");

        return Task.CompletedTask;
    }
}
