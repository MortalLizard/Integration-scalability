using Marketplace.Business.Interfaces;
using Shared;
using Shared.Contracts.OrderBook;

namespace Marketplace.Consumers;

public class OrderlineConsumer : BaseConsumer<MarketplaceOrderlineProcess>
{
    protected override string QueueName => "marketplace.order-item.process";

    public OrderlineConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer)
    {
    }

    protected override async Task HandleMessageAsync(MarketplaceOrderlineProcess command, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var orderItemLogic = serviceProvider.GetRequiredService<IOrderlineLogic>();
        await orderItemLogic.ProcessOrderline(command, cancellationToken);
    }
}
