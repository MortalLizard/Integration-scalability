using Marketplace.Business.Interfaces;
using MassTransit;
using Shared.Contracts.CreateBook;

namespace Marketplace.Consumers;

public class CreateBookConsumer(IServiceScopeFactory serviceScopeFactory) : IConsumer<BookCreate>
{
    public async Task Consume(ConsumeContext<BookCreate> context)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var orderItemLogic = scope.ServiceProvider.GetRequiredService<ICreateBookLogic>();
        await orderItemLogic.CreateBook(context.Message, context.CancellationToken);
    }
}
