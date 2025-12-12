using System.Text.Json;
using Marketplace.Business.Interfaces;
using Marketplace.Contracts.Commands;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace Marketplace.Consumers;

public class CreateBookConsumer : BaseConsumer<CreateBook>
{
    protected override string QueueName => "marketplace.create-book";

    public CreateBookConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer)
    {
    }

    protected override async Task HandleMessageAsync(CreateBook command, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var createBookLogic = serviceProvider.GetRequiredService<ICreateBookLogic>();
        await createBookLogic.CreateBook(command, cancellationToken);
    }
}
