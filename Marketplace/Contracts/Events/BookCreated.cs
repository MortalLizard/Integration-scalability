using System;
using Marketplace.Contracts.Dtos;

namespace Marketplace.Contracts.Events;

public sealed record BookCreated
{
    public required BookDto Book { get; set; }
    public required DateTime OccurredAt { get; set; }

    public BookCreated()
    {
        OccurredAt = DateTime.UtcNow;
    }
}
