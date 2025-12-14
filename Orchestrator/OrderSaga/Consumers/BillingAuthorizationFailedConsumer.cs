using Shared;
using Shared.Contracts.OrderBook.Billing;

namespace Orchestrator.OrderSaga.Consumers;

public sealed class BillingAuthorizationFailedConsumer : BaseConsumer<BillingAuthorizationFailed>
{
    protected override string QueueName => "billing.authorization.failed";

    public BillingAuthorizationFailedConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer) { }

    protected override async Task HandleMessageAsync(BillingAuthorizationFailed message, IServiceProvider sp, CancellationToken ct)
    {
        var pm = sp.GetRequiredService<IOrderProcessManager>();
        await pm.HandlePaymentAuthorizationFailedAsync(message, ct);
    }
}
