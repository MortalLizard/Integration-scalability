using System.Text.Json;
using Inventory.Contracts.Commands;
using Inventory.Contracts.Events;
using Inventory.Database.Services;

namespace Inventory.Logic;

public class OrderItemLogic(IBookService bookService, Shared.Producer producer) : IOrderItemLogic
{
    private const string responseQueueName = "inventory.order-item.processed";

    public async Task ProcessOrderItem(OrderItemProcess orderItemProcess, CancellationToken ct = default)
    {
        var updatedBook = await bookService.UpdateStockAsync(orderItemProcess.BookId, orderItemProcess.Quantity);

        if (null == updatedBook)
        {
            //Failover scenario
        }

        var responsePayload = new OrderItemProcessed(
            CorrelationId: orderItemProcess.CorrelationId,
            BookId: orderItemProcess.BookId,
            Quantity: orderItemProcess.Quantity,
            Price: updatedBook.Price
        );

        var jsonMessage = JsonSerializer.Serialize(responsePayload);

        await producer.SendMessageAsync(responseQueueName, jsonMessage);
    }
}
