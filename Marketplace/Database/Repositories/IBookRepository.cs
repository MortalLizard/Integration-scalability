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
    /// Retrieves all books from the repository.
    /// </summary>
    Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing book and returns the updated entity.
    /// </summary>
    Task<Book> UpdateAsync(Book book, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the IsActive flag of a book by its identifier.
    /// </summary>
    Task<Book?> UpdateIsActiveAsync(Guid id);

    /// <summary>
    /// Deletes a book by identifier. Returns true if a record was deleted.
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
