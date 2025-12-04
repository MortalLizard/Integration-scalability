using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shared;

public class Consumer : IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _queueName;
    private readonly string _consumerTag;

    private Consumer(IConnection connection, IChannel channel, string queueName, string consumerTag)
    {
        _connection = connection;
        _channel = channel;
        _queueName = queueName;
        _consumerTag = consumerTag;
    }

    public static async Task<Consumer> CreateAsync(
        string queueName,
        Func<string, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "rabbit",
            Password = "rabbit_pw",
            VirtualHost = "/"
        };

        var connection = await factory.CreateConnectionAsync(cancellationToken);
        var channel = await connection.CreateChannelAsync(cancellationToken:cancellationToken);

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: cancellationToken);

        var asyncConsumer = new AsyncEventingBasicConsumer(channel);

        asyncConsumer.ReceivedAsync += async (sender, ea) =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await channel.BasicNackAsync(ea.DeliveryTag, false, true, cancellationToken);
                return;
            }

            var msg = Encoding.UTF8.GetString(ea.Body.ToArray());

            await handler(msg, cancellationToken);

            await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
        };


        var consumerTag = await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: asyncConsumer,
            cancellationToken: cancellationToken);

        Console.WriteLine($"Consumer started on queue '{queueName}', tag='{consumerTag}', prefetch=1");

        return new Consumer(connection, channel, queueName, consumerTag);
    }

    public async ValueTask DisposeAsync()
    {
        try { await _channel.BasicCancelAsync(_consumerTag); } catch { }

        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
