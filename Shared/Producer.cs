using RabbitMQ.Client;
using System.Text;
using Serilog;

namespace Shared;

public class Producer : IAsyncDisposable
{
    private readonly IChannel _channel;
    private readonly IConnection _connection;

    public Producer()
    {

        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq",
            Password = "rabbit_pw",
            UserName = "rabbit",
            VirtualHost = "/",

        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();



        Console.WriteLine($"Producer started, attached to: localhost.");
    }

    public async Task SendMessageAsync(string queueName, string message)
    {
        _channel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null).GetAwaiter().GetResult();

        var body = Encoding.UTF8.GetBytes(message);

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            mandatory: false,
            body: body);

        Log.Information($"Message sent to queue: '{queueName}', with body: '{message}'");
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
