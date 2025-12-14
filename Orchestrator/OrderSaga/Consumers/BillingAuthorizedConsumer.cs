using Shared;
using Shared.Contracts.OrderBook.Billing;

namespace Orchestrator.OrderSaga.Consumers;

public sealed class BillingAuthorizedConsumer : BaseConsumer<BillingAuthorized>
{
    protected override string QueueName => "billing.authorized";

    public BillingAuthorizedConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer) { }

    protected override async Task HandleMessageAsync(BillingAuthorized message, IServiceProvider sp, CancellationToken ct)
    {
        var pm = sp.GetRequiredService<IOrderProcessManager>();
        await pm.HandlePaymentAuthorizedAsync(message, ct);
    }
}
