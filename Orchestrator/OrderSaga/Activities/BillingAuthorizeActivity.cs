using Serilog;
using Shared;
using Shared.Contracts.OrderBook.Billing;

namespace Orchestrator.OrderSaga.Activities;

public sealed class BillingAuthorizeActivity(Producer producer) : IBillingAuthorizeActivity
{
    public async Task ExecuteAsync(Guid orderId, CancellationToken ct = default)
    {
        await producer.SendMessageAsync("billing.authorize.request", new BillingAuthorizeRequest(orderId));
        Log.Information("Dispatched billing.authorize.request OrderId={OrderId}", orderId);
    }

    public Task CompensateAsync(Guid orderId, CancellationToken ct = default)
    {
        Log.Warning("Compensate billing authorize. OrderId={OrderId}", orderId);
        return Task.CompletedTask;
    }
}
