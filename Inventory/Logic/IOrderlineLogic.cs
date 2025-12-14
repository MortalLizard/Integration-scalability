using Shared.Contracts.OrderBook;

namespace Inventory.Logic;

public interface IOrderlineLogic
{
    public Task ProcessOrderline(InventoryOrderlineProcess orderlineProcess, CancellationToken ct = default);

    Task ReleaseStockAsync(Guid bookId, int quantity, CancellationToken ct);
}
