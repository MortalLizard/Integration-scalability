using Orchestrator.Gateway.DTOs;
using Orchestrator.OrderSaga.Database.Entities;

namespace Orchestrator.OrderSaga.Utils.Mappers;

public static class OrderToSagaStateMapper
{
    extension(OrderDto dto)
    {
        public OrderSagaState ToInitialSagaState()
        {
            return new OrderSagaState
            {
                OrderId = Guid.NewGuid(),
                BuyerEmail = dto.BuyerEmail,
                LinesExpected = dto.Items.Count,
                LinesCompleted = 0,
                LinesFailed = 0,
                Status = OrderSagaStatus.PaymentAndReserve,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
