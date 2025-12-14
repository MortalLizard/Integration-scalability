using Shared.Contracts.OrderBook;

namespace Marketplace.Business.Interfaces;

public interface IOrderlineLogic
{
    public Task ProcessOrderline(MarketplaceOrderlineProcess orderlineProcess, CancellationToken ct = default);

    Task RevertIsActiveAsync(Guid bookId, CancellationToken ct);
}
