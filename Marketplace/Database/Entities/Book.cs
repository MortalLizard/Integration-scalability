namespace Marketplace.Database.Entities;

public class Book
{
    public Book()
    {

    }

    public required Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? Isbn { get; set; }
    public decimal Price { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
