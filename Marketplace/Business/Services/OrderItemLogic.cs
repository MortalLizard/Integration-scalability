using System.Text.Json;
using Marketplace.Business.Interfaces;
using Marketplace.Contracts.Commands;
using Marketplace.Contracts.Events;
using Marketplace.Database.Repositories;

using Serilog;

namespace Marketplace.Business.Services;

public class OrderItemLogic(IBookRepository bookRepository, Shared.Producer producer) : IOrderItemLogic
{
    private const string responseQueueName = "marketplace.order-item.processed";

    public async Task ProcessOrderItem(OrderItemProcess orderItemProcess, CancellationToken ct = default)
    {
        bool success = await bookRepository.UpdateIsActiveAsync(orderItemProcess.BookId, orderItemProcess.Price, ct);

        if(!success)
        {
            throw new InvalidOperationException("Price mismatch or book not active.");
        }

        var responsePayload = new OrderItemProcessed(
            CorrelationId: orderItemProcess.CorrelationId,
            BookId: orderItemProcess.BookId,
            Price: orderItemProcess.Price
        );

        string jsonMessage = JsonSerializer.Serialize(responsePayload);

        await producer.SendMessageAsync(responseQueueName, jsonMessage);
    }
}
