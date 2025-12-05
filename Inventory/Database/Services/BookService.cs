using System.Data;

using Inventory.Database.Entities;

using Microsoft.Data.SqlClient;
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

    public async Task<Book?> UpdateStockAsync(Guid id, int quantityChange, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE dbo.Books
            SET Quantity = Quantity - @quantity
            OUTPUT inserted.*
            WHERE Id = @id AND Quantity >= @quantity;
        ";

        var conn = (SqlConnection)context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.UniqueIdentifier) { Value = id });
        cmd.Parameters.Add(new SqlParameter("@quantity", SqlDbType.Int) { Value = quantityChange });

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        var book = new Book
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            Isbn = reader.GetString(reader.GetOrdinal("Isbn")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Author = reader.GetString(reader.GetOrdinal("Author")),
            Description = reader.GetString(reader.GetOrdinal("Description")),
            PublishedDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("PublishedDate"))),
            Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };

        return book;
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
