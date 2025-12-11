using Marketplace.Contracts.Commands;
using Marketplace.Database.Entities;

namespace Marketplace.Mappers;

public static class CreateBookMapper
{
    /// <summary>
    /// Maps a CreateBook command into a new Book entity.
    /// </summary>
    public static Book ToEntity(this CreateBook createBook)
    {
        var now = DateTime.UtcNow;

        return new Book
        {
            Id = Guid.NewGuid(),
            Title = createBook.Title,
            Author = createBook.Author,
            Isbn = createBook.Isbn,
            Price = createBook.Price,
            PublishedDate = createBook.PublishedDate,
            Description = createBook.Description,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
