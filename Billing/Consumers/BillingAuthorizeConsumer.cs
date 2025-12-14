using Serilog;
using Shared;
using Shared.Contracts.OrderBook.Billing;

namespace Billing.Consumers;

public sealed class BillingAuthorizeConsumer : BaseConsumer<BillingAuthorizeRequest>
{
    protected override string QueueName => "billing.authorize.request";

    public BillingAuthorizeConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer)
    {
    }

    protected override async Task HandleMessageAsync(
        BillingAuthorizeRequest command,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(150), cancellationToken);

        var success = true; // flip to simulate decline
        var producer = serviceProvider.GetRequiredService<Producer>();

        if (success)
        {
            Log.Information("Billing authorize stub succeeded. OrderId={OrderId}", command.CorrelationId);
            await producer.SendMessageAsync("billing.authorized", new BillingAuthorized(command.CorrelationId));
        }
        else
        {
            Log.Warning("Billing authorize stub failed. OrderId={OrderId}", command.CorrelationId);
            await producer.SendMessageAsync(
                "billing.authorization.failed",
                new BillingAuthorizationFailed(command.CorrelationId, "Authorization failed (stub)")
            );
        }
    }
}
