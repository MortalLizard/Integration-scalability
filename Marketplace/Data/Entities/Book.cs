namespace Marketplace.Data.Entities;

public class Book
{
    public required Guid Id { get; set; }
    public required string Title { get; set; } = null!;
    public string? Author { get; set; }
    public string? ISBN { get; set; }
    public decimal Price { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? Description { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
   
    public Book()
    {
        Id = Guid.NewGuid();
        Title = string.Empty;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }
}
