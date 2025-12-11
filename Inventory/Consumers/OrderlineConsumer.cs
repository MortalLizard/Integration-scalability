using Inventory.Logic;
using MassTransit;

using Serilog;

using Shared.Contracts.OrderBook;

namespace Inventory.Consumers;

public class OrderlineConsumer(IServiceScopeFactory serviceScopeFactory) : IConsumer<InventoryOrderlineProcess>
{
    public async Task Consume(ConsumeContext<InventoryOrderlineProcess> context)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var orderItemLogic = scope.ServiceProvider.GetRequiredService<IOrderlineLogic>();
        await orderItemLogic.ProcessOrderItem(context.Message, context.CancellationToken);
    }
}
