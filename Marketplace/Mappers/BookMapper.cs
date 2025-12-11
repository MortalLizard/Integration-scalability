using Marketplace.Database.Entities;
using Shared.Contracts.CreateBook;

namespace Marketplace.Mappers;

public static class CreateBookMapper
{
    public static Book ToEntity(this BookCreate cmd)
    {
        var now = DateTime.UtcNow;

        return new Book
        {
            Id = Guid.NewGuid(),
            Title = cmd.Title,
            Author = cmd.Author,
            Isbn = cmd.Isbn,
            Price = cmd.Price,
            PublishedDate = cmd.PublishedDate,
            Description = cmd.Description,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
