using Marketplace.Database.Entities;

namespace Marketplace.Data.Repositories;

public interface IBookRepository
{
    /// <summary>
    /// Adds a new book to the repository and returns the created entity.
    /// </summary>
    Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a book by its identifier, or null if not found.
    /// </summary>
    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all books from the repository.
    /// </summary>
    Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing book and returns the updated entity.
    /// </summary>
    Task<Book> UpdateAsync(Book book, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a book by identifier. Returns true if a record was deleted.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
