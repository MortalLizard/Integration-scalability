using Inventory.Logic;
using Shared;
using Shared.Contracts.OrderBook;

namespace Inventory.Consumers;

public class OrderItemConsumer : BaseConsumer<InventoryOrderlineProcess>
{
    protected override string QueueName => "inventory.order-item.process";

    public OrderItemConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer)
    {
    }

    protected override async Task HandleMessageAsync(InventoryOrderlineProcess command, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var orderItemLogic = serviceProvider.GetRequiredService<IOrderlineLogic>();
        await orderItemLogic.ProcessOrderline(command, cancellationToken);
    }
}
