using Inventory.Contracts.Commands;
using Inventory.Logic;
using Shared;

namespace Inventory.Consumers;

public class OrderItemConsumer : BaseConsumer<OrderItemProcess>
{
    protected override string QueueName => "inventory.order-item.process";

    public OrderItemConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer)
    {
    }

    protected override async Task HandleMessageAsync(OrderItemProcess command, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var orderItemLogic = serviceProvider.GetRequiredService<IOrderItemLogic>();
        await orderItemLogic.ProcessOrderItem(command, cancellationToken);
    }
}
