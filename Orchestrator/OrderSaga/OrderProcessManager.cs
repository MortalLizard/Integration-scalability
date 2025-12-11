using Orchestrator.Gateway.DTOs;
using Orchestrator.Utils.Enrichers;
using Shared;

namespace Orchestrator.OrderSaga;

public class OrderProcessManager(Producer producer) : IOrderProcessManager
{
    public async Task HandleNewOrderAsync(OrderDto dto, CancellationToken cancellationToken = default)
    {
        if (dto is null)
            throw new ArgumentNullException(nameof(dto));

        var orderMessage = dto.ToOrderMessage();

        // persist order here
        foreach (var orderLine in dto.Items)
        {
            if (orderLine.Marketplace)
            {
                var msg = orderLine.ToMarketplaceOrderlineProcess(orderMessage.OrderId);

                await producer.SendMessageAsync(
                    "marketplace.order-item.process", msg);
            }
            else
            {
                var msg = orderLine.ToInventoryOrderlineProcess(orderMessage.OrderId);

                await producer.SendMessageAsync("inventory.order-item.process", msg);
            }
        }

    }
}
