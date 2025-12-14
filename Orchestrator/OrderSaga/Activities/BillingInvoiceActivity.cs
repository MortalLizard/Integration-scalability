using Serilog;
using Shared;
using Shared.Contracts.OrderBook.Billing;

namespace Orchestrator.OrderSaga.Activities;

public sealed class BillingInvoiceActivity(Producer producer) : IBillingInvoiceActivity
{
    public async Task ExecuteAsync(Guid orderId, CancellationToken ct = default)
    {
        await producer.SendMessageAsync("billing.invoice.request", new BillingInvoiceRequest(orderId));
        Log.Information("Dispatched billing.invoice.request OrderId={OrderId}", orderId);
    }

    public Task CompensateAsync(Guid orderId, CancellationToken ct = default)
    {
        Log.Warning("Compensate billing invoice. OrderId={OrderId}", orderId);
        return Task.CompletedTask;
    }
}
