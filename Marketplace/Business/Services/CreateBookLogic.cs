using Inventory.Consumers;
using Marketplace.Business.Interfaces;
using Marketplace.Contracts.Commands;
using Marketplace.Data.Repositories;
using Marketplace.Mappers;

namespace Marketplace.Business.Services;

public class CreateBookLogic(IBookRepository bookRepository, Shared.Producer producer) : ICreateBookLogic
{
    private readonly IBookRepository _bookRepository = bookRepository;
    private readonly Shared.Producer _producer = producer;

    public async Task CreateBook(CreateBook createBook, CancellationToken ct = default)
    {
        var book = createBook.ToEntity();

        var createdBook = await _bookRepository.AddAsync(book, ct);

        if (null == createdBook)
        {
            // Handle negative outcome with _producer
        }

        // Handle positive outcome _producer
    }
}
