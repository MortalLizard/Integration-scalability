using Inventory.Database.Services;
using Shared.Contracts.Mappers;
using Shared.Contracts.OrderBook;

namespace Inventory.Logic;

public class OrderlineLogic(IBookService bookService, Shared.Producer producer) : IOrderlineLogic
{
    private const string responseQueueName = "inventory.order-item.processed";
    private const string failedQueueName = "inventory.order-item.process.failed";

    public async Task ProcessOrderline(InventoryOrderlineProcess orderlineProcess, CancellationToken ct = default)
    {
        bool success = await bookService.UpdateStockAsync(orderlineProcess.BookId, orderlineProcess.Quantity, orderlineProcess.Price, ct);

        if (!success)
        {
            await producer.SendMessageAsync(failedQueueName, orderlineProcess.ToInventoryOrderlineProcessFailed());
            return;
        }

        await producer.SendMessageAsync(responseQueueName, orderlineProcess.ToInventoryOrderlineProcessed());
    }

    public async Task ReleaseStockAsync(Guid bookId, int quantity, CancellationToken ct)
    {
        await bookService.ReleaseStockAsync(bookId, quantity, ct);
    }
}
