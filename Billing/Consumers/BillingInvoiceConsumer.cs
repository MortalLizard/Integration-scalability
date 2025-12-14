using Serilog;
using Shared;
using Shared.Contracts.OrderBook.Billing;

namespace Billing.Consumers;

public sealed class BillingInvoiceConsumer : BaseConsumer<BillingInvoiceRequest>
{
    protected override string QueueName => "billing.invoice.request";

    public BillingInvoiceConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer)
    {
    }

    protected override async Task HandleMessageAsync(
        BillingInvoiceRequest command,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(120), cancellationToken);

        var success = true;
        var producer = serviceProvider.GetRequiredService<Producer>();

        if (success)
        {
            Log.Information("Billing invoice stub succeeded. OrderId={OrderId}", command.CorrelationId);
            await producer.SendMessageAsync("billing.invoiced", new BillingInvoiced(command.CorrelationId));
        }
        else
        {
            Log.Warning("Billing invoice stub failed. OrderId={OrderId}", command.CorrelationId);
            await producer.SendMessageAsync(
                "billing.invoice.failed",
                new BillingInvoiceFailed(command.CorrelationId, "Invoice failed (stub)")
            );
        }
    }
}
