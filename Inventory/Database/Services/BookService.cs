using System.Data;
using Inventory.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Database.Services;

public class BookService : IBookService
{
    private readonly InventoryDbContext _context;

    public BookService(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<List<Book>> GetAllAsync()
    {
        return await _context.Books.AsNoTracking().ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(Guid id)
    {
        return await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task CreateAsync(Book book)
    {
        book.CreatedAt = DateTime.UtcNow;

        _context.Books.Add(book);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        try
        {
            var existingBook = await _context.Books
                .FromSqlInterpolated($"SELECT * FROM [Books] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {book.Id}")
                .FirstOrDefaultAsync();

            if (existingBook == null) return false;

            existingBook.Isbn = book.Isbn;
            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.Description = book.Description;
            existingBook.PublishedDate = book.PublishedDate;
            existingBook.Quantity = book.Quantity;
            existingBook.Price = book.Price;
            existingBook.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Book?> UpdateStockAsync(Guid id, int quantityChange)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        try
        {
            var book = await _context.Books
                .FromSqlInterpolated($"SELECT * FROM [Books] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {id}")
                .FirstOrDefaultAsync();

            if (book == null) return null;

            if (book.Quantity - quantityChange < 0)
            {
                await transaction.RollbackAsync();
                return null;
            }

            book.Quantity -= quantityChange;
            book.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return book;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        try
        {
            var book = await _context.Books
                .FromSqlInterpolated($"SELECT * FROM [Books] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {id}")
                .FirstOrDefaultAsync();

            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
