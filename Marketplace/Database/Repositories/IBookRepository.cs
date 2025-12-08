using Marketplace.Database.Entities;
namespace Marketplace.Database.Repositories;

public interface IBookRepository
{
    /// <summary>
    /// Adds a new book to the repository and returns the created entity.
    /// </summary>
    Task<Book?> CreateAsync(Book book, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a book by its identifier, or null if not found.
    /// </summary>
    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the IsActive flag of a book by its identifier.
    /// </summary>
    Task<bool> UpdateIsActiveAsync(Guid id, decimal expectedPrice, CancellationToken cancellationToken = default);
}
