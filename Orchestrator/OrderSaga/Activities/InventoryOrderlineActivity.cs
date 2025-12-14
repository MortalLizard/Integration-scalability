using Orchestrator.OrderSaga.Database.Entities;
using Orchestrator.OrderSaga.Utils.Mappers;
using Serilog;
using Shared;
using Shared.Contracts.OrderBook;

namespace Orchestrator.OrderSaga.Activities;

public sealed class InventoryOrderLineActivity(Producer producer) : IInventoryOrderLineActivity
{
    public async Task ExecuteAsync(OrderSagaLine line, CancellationToken ct = default)
    {
        var msg = line.ToInventoryOrderlineProcess();
        await producer.SendMessageAsync("inventory.order-item.process", msg);

        Log.Information("Dispatched inventory.order-item.process OrderId={OrderId} LineId={LineId} BookId={BookId} Quantity={Quantity}",
            msg.CorrelationId, msg.LineId, msg.BookId, msg.Quantity);
    }

    public async Task CompensateAsync(OrderSagaLine line, CancellationToken ct = default)
    {
        if (line.Quantity is null)
            return;

        var msg = new InventoryOrderlineCompensate(
            CorrelationId: line.OrderId,
            LineId: line.LineId,
            BookId: line.BookId,
            Quantity: line.Quantity.Value
        );

        await producer.SendMessageAsync("inventory.order-item.compensate", msg);

        Log.Warning("Dispatched inventory.order-item.compensate OrderId={OrderId} LineId={LineId} BookId={BookId} Quantity={Quantity}",
            msg.CorrelationId, msg.LineId, msg.BookId, msg.Quantity);
    }
}
