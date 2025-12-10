namespace Search.Models;

public class Book
{
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string Isbn { get; set; }
    public required decimal Price { get; set; }
    public required DateTime PublishedDate { get; set; }
    public string? Description { get; set; }
    public required string Origin { get; set; }
    public string? SellerId { get; set; }
}
