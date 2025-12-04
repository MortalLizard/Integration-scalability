using System;

namespace Marketplace.Contracts.Messages;

public sealed class CreateBookDto
{
    public required string Title { get; set; }
    public string? Author { get; set; }
    public string? ISBN { get; set; }
    public required decimal Price { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
