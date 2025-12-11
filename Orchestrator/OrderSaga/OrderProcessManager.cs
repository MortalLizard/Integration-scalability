using Orchestrator.Gateway.DTOs;
using Orchestrator.OrderSaga.Repository;
using Orchestrator.Utils.Enrichers;
using Orchestrator.Utils.Mappers;

using Serilog;

using Shared;

namespace Orchestrator.OrderSaga;

public class OrderProcessManager(Producer producer, IOrderSagaRepository sagaRepository) : IOrderProcessManager
{
    public async Task HandleNewOrderAsync(OrderDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var saga = dto.ToInitialSagaState();
        await sagaRepository.SaveAsync(saga, ct);

        Log.Information("Order saga started. OrderId={OrderId}, BuyerEmail={BuyerEmail}, Items={ItemCount}", saga.OrderId, saga.BuyerEmail, saga.LinesExpected);

        foreach (var orderLine in dto.Items)
        {
            if (orderLine.Marketplace)
            {
                var msg = orderLine.ToMarketplaceOrderlineProcess(saga.OrderId);

                await producer.SendMessageAsync("marketplace.order-item.process", msg);

                Log.Information(
                    "Dispatched marketplace.order-item.process for OrderId={OrderId}, BookId={BookId}", msg.CorrelationId, msg.BookId); }
            else
            {
                var msg = orderLine.ToInventoryOrderlineProcess(saga.OrderId);

                await producer.SendMessageAsync("inventory.order-item.process", msg);

                Log.Information("Dispatched inventory.order-item.process for OrderId={OrderId}, BookId={BookId}, Quantity={Quantity}", msg.CorrelationId, msg.BookId, msg.Quantity);
            }
        }

        saga.Status = OrderSagaStatus.WaitingForLineResults;
        saga.Touch();
        await sagaRepository.SaveAsync(saga, ct);

        Log.Information("Saga for OrderId={OrderId} moved to {Phase}. Waiting for line results ({Expected} lines).", saga.OrderId, saga.Status, saga.LinesExpected);
    }
}
