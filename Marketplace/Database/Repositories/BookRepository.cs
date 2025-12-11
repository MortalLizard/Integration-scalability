using System.Data;

using Marketplace.Database.DBContext;
using Marketplace.Database.Entities;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Database.Repositories;

public class BookRepository(MarketplaceDbContext dbContext) : IBookRepository
{
    private readonly MarketplaceDbContext dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<Book?> CreateAsync(Book book, CancellationToken cancellationToken = default)
    {
        if (book.CreatedAt == default)
        {
            book.CreatedAt = DateTime.UtcNow;
        }

        await dbContext.Books.AddAsync(book, cancellationToken);
        int entryCount = await dbContext.SaveChangesAsync(cancellationToken);

        return entryCount > 0 ? book : null;
    }

    public Book? GetById(Guid id)
    {
        return dbContext.Books
            .AsNoTracking()
            .FirstOrDefault(b => b.Id == id);
    }

    public async Task<bool> UpdateIsActiveAsync(Guid id, decimal expectedPrice, CancellationToken cancellationToken = default)
    {
        int affected = await dbContext.Books
            .Where(b => b.Id == id &&
                        b.IsActive &&
                        b.Price == expectedPrice)
            .ExecuteUpdateAsync(setters => setters.SetProperty(b => b.IsActive, b => false), cancellationToken);

        return affected == 1;
    }
}
