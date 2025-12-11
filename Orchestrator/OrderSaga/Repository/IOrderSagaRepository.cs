using Orchestrator.OrderSaga;

namespace Orchestrator.OrderSaga.Repository;

public interface IOrderSagaRepository
{
    Task<OrderSagaState?> GetAsync(Guid orderId, CancellationToken ct = default);
    Task SaveAsync(OrderSagaState state, CancellationToken ct = default);
}
