using Orchestrator.OrderSaga.Database.Entities;

namespace Orchestrator.OrderSaga.Database.Repository;

public interface IOrderSagaRepository
{
    Task SaveAsync(OrderSagaState state, CancellationToken ct = default);

    Task<OrderSagaState?> GetAsync(Guid orderId, CancellationToken ct = default);

    Task AddLinesAsync(IReadOnlyCollection<OrderSagaLine> lines, CancellationToken ct = default);

    Task<bool> TryAdvanceStatusAsync(Guid orderId, OrderSagaStatus from, OrderSagaStatus to, CancellationToken ct = default);

    Task<bool> TryMarkLineReservedAsync(Guid orderId, Guid lineId, CancellationToken ct = default);

    Task<bool> TryMarkLineFailedAsync(Guid orderId, Guid lineId, string reason, CancellationToken ct = default);

    Task<int> CountReservedAsync(Guid orderId, CancellationToken ct = default);

    Task<int> CountFailedAsync(Guid orderId, CancellationToken ct = default);

    Task<IReadOnlyList<OrderSagaLine>> GetLinesByStatusAsync(Guid orderId, OrderSagaLineStatus status, CancellationToken ct = default);

    Task<bool> TryMarkLineCompensationSentAsync(Guid orderId, Guid lineId, CancellationToken ct = default);
}
