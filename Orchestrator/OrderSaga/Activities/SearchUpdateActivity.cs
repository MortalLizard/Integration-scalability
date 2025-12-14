using Serilog;
using Shared;

namespace Orchestrator.OrderSaga.Activities;

public sealed class SearchUpdateActivity(Producer producer) : ISearchUpdateActivity
{
    public async Task ExecuteAsync(Guid orderId, CancellationToken ct = default)
    {
        await producer.SendMessageAsync("search.update.request", new { correlation_id = orderId });
        Log.Information("Dispatched search.update.request OrderId={OrderId}", orderId);
    }

    public Task CompensateAsync(Guid orderId, CancellationToken ct = default)
    {
        Log.Warning("Compensate search update. OrderId={OrderId}", orderId);
        return Task.CompletedTask;
    }
}
