using Orchestrator.Gateway.DTOs;

using Shared.Contracts.OrderBook;

namespace Orchestrator.Utils.Enrichers;

public static class OrderEnricher
{
    extension(OrderDto dto)
    {
        public OrderProcess ToOrderMessage()
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new OrderProcess(
                OrderId: Guid.NewGuid(),
                BuyerEmail: dto.BuyerEmail,
                TotalItems: dto.Items.Count
            );
        }
    }
}
