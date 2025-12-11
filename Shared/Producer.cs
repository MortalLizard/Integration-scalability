using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Serilog;

namespace Shared;

public class Producer(IConnection connection)
{
    public async Task SendMessageAsync<T>(string queueName, T message)
    {
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        byte[] body;

        if (message is string s)
        {
            body = Encoding.UTF8.GetBytes(s);
        }
        else
        {
            body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        }

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            mandatory: false,
            body: body
        );

        Log.Information("Message sent to queue: '{QueueName}', with payload: {Payload}",queueName, body);
    }
}
