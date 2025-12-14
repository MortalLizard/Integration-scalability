using Marketplace.Business.Interfaces;
using Serilog;
using Shared;
using Shared.Contracts.OrderBook;

namespace Marketplace.Consumers;

public sealed class MarketplaceOrderlineCompensateConsumer : BaseConsumer<MarketplaceOrderlineCompensate>
{
    protected override string QueueName => "marketplace.order-item.compensate";

    public MarketplaceOrderlineCompensateConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer)
    {
    }

    protected override async Task HandleMessageAsync(MarketplaceOrderlineCompensate command, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var bookRepository = serviceProvider.GetRequiredService<IOrderlineLogic>();

        await bookRepository.RevertIsActiveAsync(command.BookId, cancellationToken);

        Log.Information("Marketplace compensation applied. OrderId={OrderId} LineId={LineId} BookId={BookId}",
            command.CorrelationId, command.LineId, command.BookId);
    }
}
