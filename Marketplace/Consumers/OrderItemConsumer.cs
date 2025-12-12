using System.Text.Json;

using Marketplace.Business.Interfaces;
using Marketplace.Contracts.Commands;

using Shared;

namespace Marketplace.Consumers;

public class OrderItemConsumer : BaseConsumer<OrderItemProcess>
{
    protected override string QueueName => "marketplace.order-item.process";

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
