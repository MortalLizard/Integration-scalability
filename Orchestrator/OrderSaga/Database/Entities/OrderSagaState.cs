namespace Orchestrator.OrderSaga.Database.Entities;

public enum OrderSagaStatus
{
    NewOrderReceived = 0,
    LinesDispatched = 1,
    WaitingForLineResults = 2,
    PaymentAndReserve = 3,
    InvoiceShipSearch = 4,
    Completed = 5,
    Failed = 6
}

public sealed class OrderSagaState
{
    public Guid OrderId { get; set; }

    public string BuyerEmail { get; set; } = null!;

    // line-processing phase
    public int LinesExpected { get; set; }
    public int LinesCompleted { get; set; }
    public int LinesFailed { get; set; }

    public OrderSagaStatus Status { get; set; } = OrderSagaStatus.NewOrderReceived;

    public bool PaymentAuthorized { get; set; }
    public bool InventoryReserved { get; set; }
    public bool Invoiced { get; set; }
    public bool Shipped { get; set; }
    public bool SearchUpdated { get; set; }

    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
