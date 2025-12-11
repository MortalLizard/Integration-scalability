using Gateway.DTOs;
using Shared.Contracts.OrderBook;

namespace Gateway.Utils.Mappers;

public static class OrderMapper
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
