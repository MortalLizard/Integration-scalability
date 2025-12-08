namespace Shared.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

public static class RabbitServiceExtension
{
    public static IServiceCollection AddRabbitInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory
            {
                HostName = "rabbitmq",
                Password = "rabbit_pw",
                UserName = "rabbit",
                VirtualHost = "/",

            };

            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        services.AddTransient<Producer>();

        return services;
    }
}
