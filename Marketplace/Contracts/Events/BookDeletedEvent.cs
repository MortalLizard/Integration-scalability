using System;

namespace Marketplace.Contracts.Events;

public sealed class BookDeletedEvent
{
    public required Guid BookId { get; set; }
    public required DateTime OccurredAt { get; set; }
    public bool HardDelete { get; set; }

    public BookDeletedEvent()
    {
        OccurredAt = DateTime.UtcNow;
    }
}
