using Orchestrator.OrderSaga.Database.Entities;

namespace Orchestrator.OrderSaga.Activities;

public interface IMarketplaceOrderLineActivity
{
    Task ExecuteAsync(OrderSagaLine line, CancellationToken ct = default);
    Task CompensateAsync(OrderSagaLine line, CancellationToken ct = default);
}
