namespace Orchestrator.OrderSaga.Activities;

public interface ISearchUpdateActivity
{
    Task ExecuteAsync(Guid orderId, CancellationToken ct = default);
    Task CompensateAsync(Guid orderId, CancellationToken ct = default);
}
