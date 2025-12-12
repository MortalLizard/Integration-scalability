namespace Orchestrator.OrderSaga.Database.Entities;

public enum OrderSagaLineStatus
{
    Pending = 0,
    Reserved = 1,
    Failed = 2,
    CompensationSent = 3,
    Compensated = 4
}

public sealed class OrderSagaLine
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public Guid LineId { get; set; }

    public bool Marketplace { get; set; }

    public Guid BookId { get; set; }
    public int? Quantity { get; set; }
    public decimal Price { get; set; }

    public OrderSagaLineStatus Status { get; set; } = OrderSagaLineStatus.Pending;
    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public void Touch() => UpdatedAt = DateTime.UtcNow;
}
