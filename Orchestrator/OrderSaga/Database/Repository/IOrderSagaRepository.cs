using Orchestrator.OrderSaga.Database.Entities;

namespace Orchestrator.OrderSaga.Database.Repository;

public interface IOrderSagaRepository
{
    Task<OrderSagaState?> GetAsync(Guid orderId, CancellationToken ct = default);
    Task SaveAsync(OrderSagaState state, CancellationToken ct = default);
}
