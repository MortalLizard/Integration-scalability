using System.Text.Json;
using Marketplace.Business.Interfaces;
using Marketplace.DTOs;
using Shared;

namespace Inventory.Consumers;

public class CreateBookConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private Consumer? _consumer;
    private readonly string _queueName = "marketplace.create-book";

    public CreateBookConsumer(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer = await Consumer.CreateAsync(
            queueName: _queueName,
            handler: async (message, ct) =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var createBookLogic = scope.ServiceProvider.GetRequiredService<ICreateBookLogic>();

                    var dto = JsonSerializer.Deserialize<CreateBook>(message)!;
                    await createBookLogic.CreateBook(dto, ct);
                }
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
        if (_consumer is not null)
        {
            await _consumer.DisposeAsync();
            _consumer = null;
        }

        await base.StopAsync(cancellationToken);
    }
}
