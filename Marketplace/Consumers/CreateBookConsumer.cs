using Marketplace.Business.Interfaces;
using Shared;
using Shared.Contracts.CreateBook;

namespace Marketplace.Consumers;

public class CreateBookConsumer : BaseConsumer<MarketplaceBookCreate>
{
    protected override string QueueName => "marketplace.create-book";

    public CreateBookConsumer(IServiceScopeFactory serviceScopeFactory, IConsumer consumer)
        : base(serviceScopeFactory, consumer)
    {
    }

    protected override async Task HandleMessageAsync(MarketplaceBookCreate command, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var createBookLogic = serviceProvider.GetRequiredService<ICreateBookLogic>();
        await createBookLogic.CreateBook(command, cancellationToken);
    }
}
