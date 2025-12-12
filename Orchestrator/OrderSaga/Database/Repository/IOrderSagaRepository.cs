using Orchestrator.OrderSaga.Database.Entities;

namespace Orchestrator.OrderSaga.Database.Repository;

public interface IOrderSagaRepository
{
    Task SaveAsync(OrderSagaState state, CancellationToken ct = default);
}
