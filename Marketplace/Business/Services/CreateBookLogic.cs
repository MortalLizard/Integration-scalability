using System.Text.Json;
using Marketplace.Business.Interfaces;
using Marketplace.Contracts.Commands;
using Marketplace.Contracts.Events;
using Marketplace.Database.Repositories;
using Marketplace.Mappers;

namespace Marketplace.Business.Services;

public class CreateBookLogic(IBookRepository bookRepository, Shared.Producer producer) : ICreateBookLogic
{
    private const string ResponseQueueName = "marketplace.book-created";

    public async Task CreateBook(CreateBook createBook, CancellationToken ct = default)
    {
        var book = createBook.ToEntity();

        var createdBook = await bookRepository.CreateAsync(book, ct);

        if (null == createdBook)
        {
            // Handle negative outcome
        }

        var responsePayload = new BookCreated(
            Id: createdBook.Id,
            Title: createBook.Title,
            Author: createBook.Author,
            Isbn: createBook.Isbn,
            Price: createBook.Price,
            PublishedDate: createBook.PublishedDate,
            Description: createBook.Description,
            IsActive: createdBook.IsActive,
            Timestamp: DateTime.UtcNow
        );

        var jsonMessage = JsonSerializer.Serialize(responsePayload);

        await producer.SendMessageAsync(ResponseQueueName, jsonMessage);
    }
}
