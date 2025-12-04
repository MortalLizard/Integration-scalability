using System;
using Marketplace.Contracts.Dtos;

namespace Marketplace.Contracts.Events;

public sealed class BookCreatedEvent
{
    public required BookDto Book { get; set; }
    public required DateTime OccurredAt { get; set; }

    public BookCreatedEvent()
    {
        OccurredAt = DateTime.UtcNow;
    }
}
