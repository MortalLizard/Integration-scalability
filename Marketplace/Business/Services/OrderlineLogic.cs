using System.Text.Json;
using Marketplace.Business.Interfaces;
using Marketplace.Database.Repositories;

using MassTransit;

using Serilog;

using Shared.Contracts.Mappers;
using Shared.Contracts.OrderBook;

namespace Marketplace.Business.Services;

public class OrderlineLogic(IBookRepository bookRepository, IPublishEndpoint publishEndpoint) : IOrderlineLogic
{
    public async Task ProcessOrderline(MarketplaceOrderlineProcess orderlineProcess, CancellationToken ct = default)
    {
        bool success = await bookRepository.UpdateIsActiveAsync(orderlineProcess.BookId, orderlineProcess.Price, ct);

        if(!success)
        {
            Log.Error("Failed to update active state for book {BookId}", orderlineProcess.BookId);
            await publishEndpoint.Publish(orderlineProcess.ToMarketplaceOrderlineProcessFailed(), ct);
        }

        await publishEndpoint.Publish(orderlineProcess.ToMarketplaceOrderlineProcessed(), ct);
    }
}
