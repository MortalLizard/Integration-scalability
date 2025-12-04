using System;

namespace Marketplace.Contracts.Messages;

public sealed class DeleteBookDto
{
    public required Guid Id { get; set; }

    // Soft delete by default; set to true to request hard delete.
    public bool HardDelete { get; set; } = false;
}
