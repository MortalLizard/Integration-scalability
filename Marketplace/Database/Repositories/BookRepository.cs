using Marketplace.Database.DBContext;
using Marketplace.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Database.Repositories;

public class BookRepository : IBookRepository
{
    private readonly MarketplaceDbContext dbContext;

    public BookRepository(MarketplaceDbContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

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

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Book?> UpdateIsActiveAsync(Guid id)
    {
        var updated = await dbContext.Books
            .FromSqlInterpolated($"""
                                      UPDATE [Books]
                                      SET
                                          [IsActive] = false,
                                          [UpdatedAt] = {DateTime.UtcNow}
                                      OUTPUT inserted.*
                                      WHERE [Id] = {id} AND [IsActive] = true;
                                  """)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return updated;
    }
}
