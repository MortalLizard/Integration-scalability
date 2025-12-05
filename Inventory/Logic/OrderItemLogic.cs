using System.Text.Json;
using Inventory.Database.Services;
using Inventory.DTOs;

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

    public async Task ProcessOrderItem(OrderItemDto orderItemDto, CancellationToken ct = default)
    {
        var updatedBook = await _bookService.UpdateStockAsync(orderItemDto.ProductId, orderItemDto.QuantityChange);

        object responsePayload;

        if (updatedBook != null)
        {
            responsePayload = new
            {
                Type = "InventoryReserved",
                OrderId = orderItemDto.OrderId,
                ProductId = orderItemDto.ProductId,
                Quantity = orderItemDto.QuantityChange,
                Price = updatedBook.Price,
                Success = true,
                Timestamp = DateTime.UtcNow
            };
        }
        else
        {
            responsePayload = new
            {
                Type = "InventoryReservationFailed",
                OrderId = orderItemDto.OrderId,
                ProductId = orderItemDto.ProductId,
                Quantity = orderItemDto.QuantityChange,
                Success = false,
                Reason = "Insufficient Stock or Invalid Product",
                Timestamp = DateTime.UtcNow
            };
        }

        // Serialize object to JSON string because Producer expects a string
        var jsonMessage = JsonSerializer.Serialize(responsePayload);

        // Use the injected producer to send the message
        await _producer.SendMessageAsync(ResponseQueueName, jsonMessage);
    }
}
