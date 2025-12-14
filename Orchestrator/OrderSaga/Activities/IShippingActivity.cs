namespace Orchestrator.OrderSaga.Activities;

public interface IShippingActivity
{
    Task ExecuteAsync(Guid orderId, CancellationToken ct = default);
    Task CompensateAsync(Guid orderId, CancellationToken ct = default);
}
