using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Shared;

public class Producer : IAsyncDisposable
{
    private readonly IChannel _channel;
    private readonly string _queueName;
    private readonly int _sleepMs;
    private readonly IConnection _connection;

    public Producer(string queueName, int sleepMs = 50)
    {
        _sleepMs = sleepMs;
        _queueName = queueName;

        // Opret forbindelse og kanal synkront ved hjælp af async init (man kan også gøre ctor asynkron, se nedenfor)
        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq", 
            Password = "rabbit_pw", 
            UserName = "rabbit",
            VirtualHost = "/",
            
        };
 
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        // Declare queue
        _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null).GetAwaiter().GetResult();

        Console.WriteLine($"Producer started, attached to: localhost.{queueName}");
    }

    public async Task SendMessageAsync(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        // Ny ikke-generisk overload i v7.x
        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: _queueName,
            mandatory: false,
            body: body);

        if (_sleepMs > 0)
            await Task.Delay(_sleepMs);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
