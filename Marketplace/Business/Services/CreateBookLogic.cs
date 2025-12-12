using System.Text.Json;
using Marketplace.Business.Interfaces;
using Marketplace.Database.Repositories;
using Marketplace.Mappers;
using Serilog;
using Shared.Contracts.CreateBook;
using Shared.Contracts.Mappers;

namespace Marketplace.Business.Services;

public class CreateBookLogic(IBookRepository bookRepository, Shared.Producer producer) : ICreateBookLogic
{
    private const string responseQueueName = "marketplace.book-created";

    public async Task CreateBook(MarketplaceBookCreate marketplaceBookCreate, CancellationToken ct = default)
    {
        var book = marketplaceBookCreate.ToEntity();

        var createdBook = await bookRepository.CreateAsync(book, ct);

        if (null == createdBook)
        {
            // Handle negative outcome
            Log.Logger.Error("Failed to create book");
        }

        await producer.SendMessageAsync(responseQueueName, marketplaceBookCreate.ToBookCreated());
    }
}
