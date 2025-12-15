using Orchestrator.OrderSaga.Database.Entities;

namespace Orchestrator.OrderSaga.Database.Repository;

public interface IOrderSagaRepository
{
    // Create saga + lines atomically
    Task CreateSagaWithLinesAsync(OrderSagaState state, IReadOnlyCollection<OrderSagaLine> lines, CancellationToken ct = default);

    // Reads
    Task<OrderSagaState?> GetSagaAsync(Guid orderId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderSagaLine>> GetLinesByStatusAsync(Guid orderId, OrderSagaLineStatus status, CancellationToken ct = default);

    // Atomic saga patches
    Task<bool> TrySetPaymentAuthorizedAsync(Guid orderId, CancellationToken ct = default);
    Task<bool> TrySetInvoicedAsync(Guid orderId, CancellationToken ct = default);
    Task<bool> TrySetShippedAsync(Guid orderId, CancellationToken ct = default);
    Task<bool> TrySetSearchUpdatedAsync(Guid orderId, CancellationToken ct = default);
    Task<bool> TrySetFailureReasonAsync(Guid orderId, string reason, CancellationToken ct = default);

    // Atomic transitions
    Task<bool> TryTransitionSagaStatusAsync(Guid orderId, OrderSagaStatus from, OrderSagaStatus to, CancellationToken ct = default);
    Task<bool> TryAdvanceToInvoiceShipSearchAsync(Guid orderId, CancellationToken ct = default);
    Task<bool> TryCompleteAsync(Guid orderId, CancellationToken ct = default);

    // Lines (used in normal path)
    Task<bool> TrySetLineReservationSuccessAsync(Guid orderId, Guid lineId, CancellationToken ct = default);
    Task<bool> TrySetLineReservationFailureAsync(Guid orderId, Guid lineId, string reason, CancellationToken ct = default);

    // Lines (used for late messages + compensation path)
    Task<bool> TryMarkLineReservedAsync(Guid orderId, Guid lineId, CancellationToken ct = default);
    Task<bool> TryMarkLineFailedAsync(Guid orderId, Guid lineId, string reason, CancellationToken ct = default);
    Task<bool> TryMarkLineCompensationSentAsync(Guid orderId, Guid lineId, CancellationToken ct = default);
}
