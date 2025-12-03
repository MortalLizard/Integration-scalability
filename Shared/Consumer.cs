using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class Consumer : IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly string _queueName;

        public string QueueName => _queueName;

        public Consumer(string hostname, string queueName, Action<string> action)
        {
            _queueName = queueName;

            Console.WriteLine($"Consumer started, listening to: {hostname}.{queueName}");

            // Opret forbindelse og kanal synkront via async (kan evt. ændres til factory-metode)
            var factory = new ConnectionFactory { HostName = hostname };
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            // Deklarér køen
            _channel.QueueDeclareAsync(
                queue: _queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null).GetAwaiter().GetResult();

            // QoS – hent én besked ad gangen
            _channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false).GetAwaiter().GetResult();

            // Async consumer
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Kald callback-action
                action(message);

                // Send ACK
                await _channel.BasicAckAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false);

                await Task.Yield(); // sikker async-afslutning
            };

            // Start consuming
            _channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: false,
                consumer: consumer).GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            await _channel.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
