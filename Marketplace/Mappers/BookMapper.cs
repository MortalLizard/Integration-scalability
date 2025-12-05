using Marketplace.Contracts.Commands;
using Marketplace.Database.Entities;

namespace Marketplace.Mappers;

public static class CreateBookMapper
{
    /// <summary>
    /// Maps a CreateBook command into a new Book entity.
    /// </summary>
    public static Book ToEntity(this CreateBook cmd)
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
