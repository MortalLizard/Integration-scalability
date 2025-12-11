namespace Orchestrator.OrderSaga.SagaStates;

public enum OrderSagaStatus
{
    Started,
    InventoryReserved,
    PaymentCompleted,
    Shipped,
    Completed,
    Failed
}

public class OrderSagaState
{
    public Guid SagaId { get; set; }
    public Guid OrderId { get; set; }

    public OrderSagaStatus Status { get; set; }

    public bool InventoryReserved { get; set; }
    public bool PaymentCaptured { get; set; }
    public bool Shipped { get; set; }

    public string? FailureReason { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

