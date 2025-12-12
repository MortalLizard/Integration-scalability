namespace Marketplace.Database.Entities;

public class Book
{
    public Book()
    {

    }

    public required Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; } = string.Empty;
    public required string Author { get; set; }
    public required string Isbn { get; set; }
    public required decimal Price { get; set; }
    public required DateTime PublishedDate { get; set; }
    public string? Description { get; set; }
    public required bool IsActive { get; set; }
    public required Guid SellerId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
