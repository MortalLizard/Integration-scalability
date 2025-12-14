using Marketplace.Business.Interfaces;
using Marketplace.Database.Repositories;
using Shared.Contracts.Mappers;
using Shared.Contracts.OrderBook;

namespace Marketplace.Business.Services;

public class OrderlineLogic(IBookRepository bookRepository, Shared.Producer producer) : IOrderlineLogic
{
    private const string responseQueueName = "marketplace.order-item.processed";
    private const string failedQueueName = "marketplace.order-item.process.failed";

    public async Task ProcessOrderline(MarketplaceOrderlineProcess orderlineProcess, CancellationToken ct = default)
    {
        bool success = await bookRepository.UpdateIsActiveAsync(orderlineProcess.BookId, orderlineProcess.Price, ct);

        if (!success)
        {
            await producer.SendMessageAsync(failedQueueName, orderlineProcess.ToMarketplaceOrderlineProcessFailed());
            return;
        }

        await producer.SendMessageAsync(responseQueueName, orderlineProcess.ToMarketplaceOrderlineProcessed());
    }
}
