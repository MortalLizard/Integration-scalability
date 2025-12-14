using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Shared;

public abstract class BaseConsumer<TCommand> : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConsumer _consumer;
    private readonly string _queueName;

    protected string QueueName => _queueName;

    protected BaseConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer, string queueName)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _consumer = consumer;
        _queueName = queueName;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.StartAsync(
            queueName: QueueName,
            handler: async (message, ct) =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dto = JsonSerializer.Deserialize<TCommand>(message)!;
                await HandleMessageAsync(dto, scope.ServiceProvider, ct);
            },
            cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    protected abstract Task HandleMessageAsync(TCommand command, IServiceProvider serviceProvider, CancellationToken cancellationToken);

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consumer.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
