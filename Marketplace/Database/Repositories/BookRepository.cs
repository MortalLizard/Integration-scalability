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

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Book?> UpdateIsActiveAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE dbo.Books
            SET IsActive = 0
            OUTPUT inserted.*
            WHERE Id = @id AND IsActive = 1;
        ";

        var conn = (SqlConnection)dbContext.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;

        cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.UniqueIdentifier) { Value = id });

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        var book = new Book
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Author = reader.GetString(reader.GetOrdinal("Author")),
            Isbn = reader.GetString(reader.GetOrdinal("Isbn")),
            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
            PublishedDate = reader.GetDateTime(reader.GetOrdinal("PublishedDate")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };

        return book;
    }

}
