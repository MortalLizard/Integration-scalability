using System;
using Marketplace.Contracts.Dtos;

namespace Marketplace.Contracts.Events;

public sealed class BookUpdatedEvent
{
    public required BookDto Book { get; set; }
    public required DateTime OccurredAt { get; set; }

    public BookUpdatedEvent()
    {
        OccurredAt = DateTime.UtcNow;
    }
}
