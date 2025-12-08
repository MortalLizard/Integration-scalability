using RabbitMQ.Client;
using System.Text;
using Serilog;

namespace Shared;

public class Producer
{
    private readonly IConnection _connection;

    public Producer(IConnection connection)
    {

        _connection = connection;

        Log.Information($"Producer started, attached to: localhost.");
    }

    public async Task SendMessageAsync(string queueName, string message)
    {
        await using var channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        channel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null).GetAwaiter().GetResult();

        byte[] body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            mandatory: false,
            body: body);

        Log.Information($"Message sent to queue: '{queueName}', with body: '{message}'");
    }
}
