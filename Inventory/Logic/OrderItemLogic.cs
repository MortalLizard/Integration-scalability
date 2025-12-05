using System.Text.Json;
using Inventory.Contracts.Commands;
using Inventory.Contracts.Events;
using Inventory.Database.Services;

namespace Inventory.Logic;

public class OrderItemLogic(IBookService bookService, Shared.Producer producer) : IOrderItemLogic
{
    private const string ResponseQueueName = "inventory.order-item.processed";

    public async Task ProcessOrderItem(OrderItemProcess orderItemProcess, CancellationToken ct = default)
    {
        var updatedBook = await bookService.UpdateStockAsync(orderItemProcess.ProductId, orderItemProcess.QuantityChange);

        if (null == updatedBook)
        {
            //Failover scenario
        }

        var responsePayload = new OrderItemProcessed(
            OrderId: orderItemProcess.OrderId,
            Email: orderItemProcess.Email,
            ProductId: orderItemProcess.ProductId,
            Quantity: orderItemProcess.QuantityChange,
            Price: updatedBook.Price,
            Portion: orderItemProcess.Portion,
            Timestamp: DateTime.UtcNow
        );

        var jsonMessage = JsonSerializer.Serialize(responsePayload);

        await producer.SendMessageAsync(ResponseQueueName, jsonMessage);
    }
}
