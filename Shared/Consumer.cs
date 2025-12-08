using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace Shared;

public class Consumer : IAsyncDisposable
{
    private readonly IConnection _connection;
    private IChannel? _channel;
    private string? _consumerTag;

    public Consumer(IConnection connection)
    {
        _connection = connection;
    }

    public async Task StartAsync(string queueName, Func<string, CancellationToken, Task> handler, CancellationToken cancellationToken = default)
    {
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: cancellationToken);

        var asyncConsumer = new AsyncEventingBasicConsumer(_channel);

        asyncConsumer.ReceivedAsync += async (sender, ea) =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await _channel.BasicNackAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken);
                return;
            }

            try
            {
                string msg = Encoding.UTF8.GetString(ea.Body.ToArray());
                await handler(msg, cancellationToken);

                await _channel.BasicAckAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    cancellationToken);

                Log.Information("Message processed: '{Message}'", msg);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing message");

                await _channel.BasicNackAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken);
            }
        };

        _consumerTag = await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: asyncConsumer,
            cancellationToken: cancellationToken);

        Log.Information("Consumer started on queue '{QueueName}', tag='{ConsumerTag}'", queueName, _consumerTag);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is null)
            return;

        try
        {
            if (_consumerTag is not null)
            {
                await _channel.BasicCancelAsync(_consumerTag);
            }
        }
        catch
        {
            // ignore errors on shutdown
        }

        await _channel.DisposeAsync();
    }
}
