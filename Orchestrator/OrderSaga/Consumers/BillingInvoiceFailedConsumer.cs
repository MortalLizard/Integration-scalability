using Shared;
using Shared.Contracts.OrderBook.Billing;

namespace Orchestrator.OrderSaga.Consumers;

public sealed class BillingInvoiceFailedConsumer : BaseConsumer<BillingInvoiceFailed>
{
    protected override string QueueName => "billing.invoice.failed";

    public BillingInvoiceFailedConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer) { }

    protected override async Task HandleMessageAsync(BillingInvoiceFailed message, IServiceProvider sp, CancellationToken ct)
    {
        var pm = sp.GetRequiredService<IOrderProcessManager>();
        await pm.HandleBillingInvoiceFailedAsync(message, ct);
    }
}
