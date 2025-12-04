using System;

namespace Marketplace.Contracts.Dtos;

public sealed class BookDto
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Author { get; set; }
    public string? ISBN { get; set; }
    public decimal Price { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? Description { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
