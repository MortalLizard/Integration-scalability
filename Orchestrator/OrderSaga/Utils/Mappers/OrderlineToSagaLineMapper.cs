using Orchestrator.Gateway.DTOs;
using Orchestrator.OrderSaga.Database.Entities;

namespace Orchestrator.OrderSaga.Utils.Mappers;

public static class OrderlineToSagaLineMapper
{
    extension(OrderlineDto dto)
    {
        public OrderSagaLine ToSagaLine(Guid orderId)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new OrderSagaLine
            {
                OrderId = orderId,
                LineId = Guid.NewGuid(),
                Marketplace = dto.Marketplace,
                BookId = dto.BookId,
                Quantity = dto.Quantity,
                Price = dto.Price,
                Status = OrderSagaLineStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
