using Orchestrator.Gateway.DTOs;

namespace Orchestrator.OrderSaga;

public interface IOrderProcessManager
{
    Task HandleNewOrderAsync(OrderDto dto, CancellationToken ct = default);
}
