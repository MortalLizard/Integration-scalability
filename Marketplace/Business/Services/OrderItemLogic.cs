using System.Text.Json;
using Marketplace.Business.Interfaces;
using Marketplace.Contracts.Commands;
using Marketplace.Contracts.Events;
using Marketplace.Database.Repositories;

namespace Marketplace.Business.Services;

public class OrderItemLogic(IBookRepository bookRepository, Shared.Producer producer) : IOrderItemLogic
{
    private const string responseQueueName = "marketplace.order-item.processed";

    public async Task ProcessOrderItem(OrderItemProcess orderItemProcess, CancellationToken ct = default)
    {
        var updatedBook = await bookRepository.UpdateIsActiveAsync(orderItemProcess.CorrelationId, ct);

        if(updatedBook == null)
        {
            // Handle negative outcome
        }

        var responsePayload = new OrderItemProcessed(
            CorrelationId: orderItemProcess.CorrelationId,
            Email: orderItemProcess.Email,
            BookId: orderItemProcess.BookId,
            Price: updatedBook.Price
        );

        var jsonMessage = JsonSerializer.Serialize(responsePayload);

        await producer.SendMessageAsync(responseQueueName, jsonMessage);
    }
}
