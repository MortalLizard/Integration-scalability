using Marketplace.Contracts.Commands;

namespace Marketplace.Business.Interfaces;

public interface IOrderItemLogic
{
    public Task ProcessOrderItem(OrderItemProcess orderItemProcess, CancellationToken ct = default);
}
