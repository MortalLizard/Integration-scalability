using Inventory.DTOs;

namespace Inventory.Logic
{
    public class OrderItemLogic : IOrderItemLogic
    {

        public async Task ProcessOrderItem(OrderItemDto orderItemDto, CancellationToken ct = default)
        {
            // Check or adjust quantity and fetch price

            // Produce new message based on result
        }
    }
}
