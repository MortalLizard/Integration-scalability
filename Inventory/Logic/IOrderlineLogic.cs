using Shared.Contracts.OrderBook;

namespace Inventory.Logic;

public interface IOrderlineLogic
{
    public Task ProcessOrderItem(InventoryOrderlineProcess orderlineProcess, CancellationToken ct = default);

}
