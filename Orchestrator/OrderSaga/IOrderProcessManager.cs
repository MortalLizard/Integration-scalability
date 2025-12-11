using Orchestrator.Gateway.DTOs;

public interface IOrderProcessManager
{
    Task HandleNewOrderAsync(OrderDto dto, CancellationToken ct = default);
}
