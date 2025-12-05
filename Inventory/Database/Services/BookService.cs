using Inventory.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Database.Services;

public class BookService(InventoryDbContext context) : IBookService
{
    public async Task<List<Book>> GetAllAsync()
    {
        return await context.Books.AsNoTracking().ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(Guid id)
    {
        return await context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task CreateAsync(Book book)
    {
        book.CreatedAt = DateTime.UtcNow;
        book.UpdatedAt = DateTime.UtcNow;

        context.Books.Add(book);
        await context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        int rows = await context.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE [Books]
            SET
                [Isbn] = {book.Isbn},
                [Title] = {book.Title},
                [Author] = {book.Author},
                [Description] = {book.Description},
                [PublishedDate] = {book.PublishedDate},
                [Quantity] = {book.Quantity},
                [Price] = {book.Price},
                [UpdatedAt] = {DateTime.UtcNow}
            WHERE [Id] = {book.Id};
        """);

        return rows > 0;
    }

    public async Task<Book?> UpdateStockAsync(Guid id, int quantityChange)
    {
        var updated = await context.Books
            .FromSqlInterpolated($"""
                                      UPDATE [Books]
                                      SET
                                          [Quantity] = [Quantity] - {quantityChange},
                                          [UpdatedAt] = {DateTime.UtcNow}
                                      OUTPUT inserted.*
                                      WHERE [Id] = {id} AND [Quantity] >= {quantityChange};
                                  """)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return updated;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        int rows = await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM [Books]
            WHERE [Id] = {id};
        ");

        return rows > 0;
    }
}
