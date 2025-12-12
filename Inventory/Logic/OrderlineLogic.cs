using System.Text.Json;
using Inventory.Database.Services;

using Shared.Contracts.Mappers;
using Shared.Contracts.OrderBook;

namespace Inventory.Logic;

public class OrderlineLogic(IBookService bookService, Shared.Producer producer) : IOrderlineLogic
{
    private const string responseQueueName = "inventory.order-item.processed";

    public async Task ProcessOrderline(InventoryOrderlineProcess orderlineProcess, CancellationToken ct = default)
    {
        bool success = await bookService.UpdateStockAsync(orderlineProcess.BookId, orderlineProcess.Quantity, orderlineProcess.Price, ct);

        if (!success)
        {
            throw new InvalidOperationException("Price mismatch or book not in stock.");
        }

        await producer.SendMessageAsync(responseQueueName, orderlineProcess.ToInventoryOrderlineProcessed());
    }
}
