using System.Text.Json;
using Marketplace.Business.Interfaces;
using Marketplace.Contracts.Commands;
using Marketplace.Contracts.Events;
using Marketplace.Database.Repositories;

namespace Marketplace.Business.Services;

public class OrderItemLogic(IBookRepository bookRepository, Shared.Producer producer) : IOrderItemLogic
{
    private const string ResponseQueueName = "marketplace.order-item.processed";

    public async Task ProcessOrderItem(OrderItemProcess orderItemProcess, CancellationToken ct = default)
    {
        var updatedBook = await bookRepository.UpdateIsActiveAsync(orderItemProcess.ProductId);

        if(updatedBook == null)
        {
            // Handle negative outcome
        }

        var responsePayload = new OrderItemProcessed(
            OrderId: orderItemProcess.OrderId,
            Email: orderItemProcess.Email,
            ProductId: orderItemProcess.ProductId,
            Price: updatedBook.Price,
            Portion: orderItemProcess.Portion,
            Timestamp: DateTime.UtcNow
        );

        var jsonMessage = JsonSerializer.Serialize(responsePayload);

        await producer.SendMessageAsync(ResponseQueueName, jsonMessage);
    }
}
