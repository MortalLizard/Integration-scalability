using Marketplace.Data.Repositories.DBContext;
using Marketplace.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Data.Repositories;

public class BookRepository : IBookRepository
{
    private readonly MarketplaceDbContext dbContext;

    public BookRepository(MarketplaceDbContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        if (book is null)
        {
            throw new ArgumentNullException(nameof(book));
        }

        if (book.Id == Guid.Empty)
        {
            book.Id = Guid.NewGuid();
        }

        if (book.CreatedAt == default)
        {
            book.CreatedAt = DateTime.UtcNow;
        }

        await dbContext.Books.AddAsync(book, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return book;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Book? entity = await dbContext.Books.FindAsync(new object[] { id }, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        dbContext.Books.Remove(entity);
        int changed = await dbContext.SaveChangesAsync(cancellationToken);
        return changed > 0;
    }

    public async Task<IEnumerable<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Books
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Book> UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        if (book is null)
        {
            throw new ArgumentNullException(nameof(book));
        }

        book.UpdatedAt = DateTime.UtcNow;

        dbContext.Books.Update(book);
        await dbContext.SaveChangesAsync(cancellationToken);

        return book;
    }
}
