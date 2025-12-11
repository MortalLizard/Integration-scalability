using Marketplace.Business.Interfaces;
using MassTransit;
using Shared.Contracts.OrderBook;

namespace Marketplace.Consumers;

public class OrderlineConsumer(IServiceScopeFactory serviceScopeFactory) : IConsumer<MarketplaceOrderlineProcess>
{
    public async Task Consume(ConsumeContext<MarketplaceOrderlineProcess> context)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var orderItemLogic = scope.ServiceProvider.GetRequiredService<IOrderlineLogic>();
        await orderItemLogic.ProcessOrderline(context.Message, context.CancellationToken);
    }
}
