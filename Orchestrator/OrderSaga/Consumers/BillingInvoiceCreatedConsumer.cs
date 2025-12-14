using Shared;
using Shared.Contracts.OrderBook.Billing;

namespace Orchestrator.OrderSaga.Consumers;

public sealed class BillingInvoicedConsumer : BaseConsumer<BillingInvoiced>
{
    protected override string QueueName => "billing.invoiced";

    public BillingInvoicedConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer) { }

    protected override async Task HandleMessageAsync(BillingInvoiced message, IServiceProvider sp, CancellationToken ct)
    {
        var pm = sp.GetRequiredService<IOrderProcessManager>();
        await pm.HandleBillingInvoicedAsync(message, ct);
    }
}
