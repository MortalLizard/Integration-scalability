using Inventory.Database.Services;
using Inventory.Logic;

using Serilog;
using Shared;
using Shared.Contracts.OrderBook;

namespace Inventory.Consumers;

public sealed class InventoryOrderlineCompensateConsumer : BaseConsumer<InventoryOrderlineCompensate>
{
    protected override string QueueName => "inventory.order-item.compensate";

    public InventoryOrderlineCompensateConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer)
    {
    }

    protected override async Task HandleMessageAsync(InventoryOrderlineCompensate command, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var bookService = serviceProvider.GetRequiredService<IOrderlineLogic>();

        await bookService.ReleaseStockAsync(command.BookId, command.Quantity, cancellationToken);

        Log.Information("Inventory compensation applied. OrderId={OrderId} LineId={LineId} BookId={BookId}",
            command.CorrelationId, command.LineId, command.BookId);
    }
}
