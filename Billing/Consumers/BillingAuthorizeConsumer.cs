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

    protected override async Task HandleMessageAsync(BillingAuthorizeRequest command, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(60), cancellationToken);

        var success = true;
        var producer = serviceProvider.GetRequiredService<Producer>();

        if (success)
        {
            await producer.SendMessageAsync("billing.authorized", new BillingAuthorized(command.CorrelationId));
        }
        else
        {
            await producer.SendMessageAsync("billing.authorization.failed", new BillingAuthorizationFailed(command.CorrelationId, "Authorization failed (stub)")
            );
        }
    }
}
