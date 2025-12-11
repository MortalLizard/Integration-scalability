using Shared.Contracts.CreateBook;
using Shared.Contracts.OrderBook;

namespace Shared.Contracts.Mappers;

public static class BookCreateMapper
{
    extension(BookCreate create)
    {
        public BookCreated ToBookCreated()
        {
            return new BookCreated(
                Id: Guid.NewGuid(),
                Title: create.Title,
                Author: create.Author,
                Isbn: create.Isbn,
                Price: create.Price,
                PublishedDate: create.PublishedDate,
                Description: create.Description
            );
        }
    }
}
