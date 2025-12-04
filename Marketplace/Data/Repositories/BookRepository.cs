
using Marketplace.Data.Entities;

namespace Marketplace.Data.Repositories;

public class BookRepository : IBookRepository
{
    public BookRepository()
    {

    }
    public Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Book> UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
