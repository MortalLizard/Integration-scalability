using System.Text.Json;
using Marketplace.Business.Interfaces;
using Marketplace.Contracts.Commands;
using Shared;

namespace Marketplace.Consumers;

public class CreateBookConsumer(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    private Consumer? consumer;
    private const string queueName = "marketplace.create-book";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        consumer = await Consumer.CreateAsync(
            queueName: queueName,
            handler: async (message, ct) =>
            {
                using var scope = serviceScopeFactory.CreateScope();
                var createBookLogic = scope.ServiceProvider.GetRequiredService<ICreateBookLogic>();

                var dto = JsonSerializer.Deserialize<CreateBook>(message)!;
                await createBookLogic.CreateBook(dto, ct);
            },
            cancellationToken: stoppingToken
        );
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (consumer is not null)
        {
            await consumer.DisposeAsync();
            consumer = null;
        }

        await base.StopAsync(cancellationToken);
    }
}
