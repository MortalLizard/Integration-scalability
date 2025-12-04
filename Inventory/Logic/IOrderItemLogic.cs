using Inventory.DTOs;

namespace Inventory.Logic;

public interface IOrderItemLogic
{
    public Task ProcessOrderItem(OrderItemDto orderItemDto, CancellationToken ct = default);

}
