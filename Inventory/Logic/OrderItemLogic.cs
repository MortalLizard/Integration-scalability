using System.Text.Json;
using Inventory.Contracts.Commands;
using Inventory.Contracts.Events;
using Inventory.Database.Services;

namespace Inventory.Logic;

public class OrderItemLogic : IOrderItemLogic
{
    private readonly IBookService _bookService;
    private readonly Shared.Producer _producer;

    private const string ResponseQueueName = "inventory.updated";

    public OrderItemLogic(IBookService bookService, Shared.Producer producer)
    {
        _bookService = bookService;
        _producer = producer;
    }

    public async Task ProcessOrderItem(OrderItemProcess orderItemProcess, CancellationToken ct = default)
    {
        var updatedBook = await _bookService.UpdateStockAsync(orderItemProcess.ProductId, orderItemProcess.QuantityChange);

        object responsePayload;

        if (updatedBook != null)
        {
            responsePayload = new OrderItemProcessSucceeded(
                OrderId: orderItemProcess.OrderId,
                ProductId: orderItemProcess.ProductId,
                Quantity: orderItemProcess.QuantityChange,
                Price: updatedBook.Price,
                Portion: orderItemProcess.Portion,
                Timestamp: DateTime.UtcNow
            );
        }
        else
        {
            responsePayload = new OrderItemProcessFailed(
                OrderId: orderItemProcess.OrderId,
                ProductId: orderItemProcess.ProductId,
                Quantity: orderItemProcess.QuantityChange,
                Portion: orderItemProcess.Portion,
                Timestamp: DateTime.UtcNow
            );
        }

        var jsonMessage = JsonSerializer.Serialize(responsePayload);

        await _producer.SendMessageAsync(ResponseQueueName, jsonMessage);
    }
}
