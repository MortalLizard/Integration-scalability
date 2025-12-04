using System;

namespace Marketplace.Contracts.Messages;

public sealed class UpdateBookDto
{
    public required Guid Id { get; set; }

    // Optional fields for partial updates. If null => no change.
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? ISBN { get; set; }
    public decimal? Price { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
