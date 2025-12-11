using Marketplace.Business.Interfaces;
using Marketplace.Database.Repositories;
using Marketplace.Mappers;
using MassTransit;
using Serilog;
using Shared.Contracts.CreateBook;
using Shared.Contracts.Mappers;

namespace Marketplace.Business.Services;

public class CreateBookLogic(IBookRepository bookRepository, IPublishEndpoint publishEndpoint) : ICreateBookLogic
{
    public async Task CreateBook(BookCreate bookCreate, CancellationToken ct = default)
    {
        var book = bookCreate.ToEntity();

        var createdBook = await bookRepository.CreateAsync(book, ct);

        if (null == createdBook)
        {
            Log.Logger.Error("Failed to create book");
            //What else?
        }

        await publishEndpoint.Publish(bookCreate.ToBookCreated(), ct);
    }
}
