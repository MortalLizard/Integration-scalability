using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Serilog;

namespace Shared.Infrastructure;

public static class RabbitServiceExtension
{
    public static IServiceCollection AddRabbitInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory
            {
                HostName = "rabbitmq",
                UserName = "rabbit",
                Password = "rabbit_pw",
                VirtualHost = "/",
            };

            const int maxRetries = 10;
            var delay = TimeSpan.FromSeconds(2);

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    Log.Information("RabbitMQ: Attempting connection {Attempt}/{Max}", attempt, maxRetries);

                    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Log.Warning(ex,
                        "RabbitMQ: Connection attempt {Attempt}/{Max} failed. Retrying in {Delay}...",
                        attempt, maxRetries, delay);

                    if (attempt == maxRetries)
                    {
                        Log.Error("RabbitMQ: Failed to connect after {Max} attempts. Throwing.", maxRetries);
                        throw;
                    }

                    Thread.Sleep(delay);
                    delay = delay * 2;
                }
            }

            throw new Exception("RabbitMQ: Connection retry logic failed unexpectedly.");
        });

        return services;
    }
}
