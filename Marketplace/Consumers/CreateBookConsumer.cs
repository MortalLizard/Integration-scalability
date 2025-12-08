using System.Text.Json;
using Marketplace.Business.Interfaces;
using Marketplace.Contracts.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;

namespace Marketplace.Consumers;

public class CreateBookConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Consumer _consumer; // the RabbitMQ consumer we built
    private const string QueueName = "marketplace.create-book";

    public CreateBookConsumer(IServiceScopeFactory serviceScopeFactory, Consumer consumer)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.StartAsync(
            queueName: QueueName,
            handler: async (message, ct) =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var createBookLogic = scope.ServiceProvider.GetRequiredService<ICreateBookLogic>();

                var dto = JsonSerializer.Deserialize<CreateBook>(message)!;
                await createBookLogic.CreateBook(dto, ct);
            },
            cancellationToken: stoppingToken
        );

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consumer.DisposeAsync(); // stop consumer properly
        await base.StopAsync(cancellationToken);
    }
}
