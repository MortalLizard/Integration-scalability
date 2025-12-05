using Marketplace.Business.Interfaces;
using Marketplace.Contracts.Commands;

namespace Marketplace.Business.Services;

public class OrderItemLogic : IOrderItemLogic
{
    public Task ProcessOrderItem(OrderItemProcess orderItemProcess, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
