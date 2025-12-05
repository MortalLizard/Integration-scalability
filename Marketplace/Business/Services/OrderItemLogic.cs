using Inventory.Consumers;

using Marketplace.Business.Interfaces;
using Marketplace.Contracts.Commands;

namespace Marketplace.Business.Services;

public class OrderItemLogic(Shared.Producer producer) : IOrderItemLogic
{

    private readonly Shared.Producer _producer = producer;

    public async Task ProcessOrderItem(OrderItemProcess orderItemProcess, CancellationToken ct = default)
    {
        //Change set active and return the updated book

        //Fetch price from updated book

        // Use _producer to handle negative/positive scenarios
    }
}
