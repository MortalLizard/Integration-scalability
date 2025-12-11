using Inventory.Database.Services;
using MassTransit;
using Serilog;
using Shared.Contracts.OrderBook;
using Shared.Contracts.Mappers;

namespace Inventory.Logic;

public class OrderlineLogic(IBookService bookService, IPublishEndpoint publishEndpoint) : IOrderlineLogic
{
    public async Task ProcessOrderItem(InventoryOrderlineProcess orderlineProcess, CancellationToken ct = default)
    {
        bool success = await bookService.UpdateStockAsync(orderlineProcess.BookId, orderlineProcess.Quantity, orderlineProcess.Price, ct);

        if (!success)
        {
            Log.Error("Failed to update stock for book {BookId}", orderlineProcess.BookId);
            await publishEndpoint.Publish(orderlineProcess.ToInventoryOrderlineProcessFailed(), ct);
        }

        await publishEndpoint.Publish(orderlineProcess.ToInventoryOrderlineProcessed(), ct);
    }
}
