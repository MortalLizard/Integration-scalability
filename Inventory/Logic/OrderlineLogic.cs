using System.Text.Json;
using Inventory.Contracts.Commands;
using Inventory.Contracts.Events;
using Inventory.Database.Services;

namespace Inventory.Logic;

public class OrderlineLogic(IBookService bookService, Shared.Producer producer) : IOrderlineLogic
{
    private const string responseQueueName = "inventory.order-item.processed";

    public async Task ProcessOrderItem(InventoryOrderlineProcess orderItemProcess, CancellationToken ct = default)
    {
        bool success = await bookService.UpdateStockAsync(orderItemProcess.BookId, orderItemProcess.Quantity, orderItemProcess.Price, ct);

        if (!success)
        {
            throw new InvalidOperationException("Price mismatch or book not in stock.");
        }

        var responsePayload = new OrderItemProcessed(
            CorrelationId: orderItemProcess.CorrelationId,
            BookId: orderItemProcess.BookId,
            Quantity: orderItemProcess.Quantity,
            Price: orderItemProcess.Price
        );

        string jsonMessage = JsonSerializer.Serialize(responsePayload);

        await producer.SendMessageAsync(responseQueueName, jsonMessage);
    }
}
