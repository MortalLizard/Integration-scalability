using Orchestrator.OrderSaga.Database.Entities;
using Orchestrator.OrderSaga.Utils.Mappers;
using Serilog;
using Shared;
using Shared.Contracts.OrderBook;

namespace Orchestrator.OrderSaga.Activities;

public sealed class MarketplaceOrderLineActivity(Producer producer) : IMarketplaceOrderLineActivity
{
    public async Task ExecuteAsync(OrderSagaLine line, CancellationToken ct = default)
    {
        var msg = line.ToMarketplaceOrderlineProcess();
        await producer.SendMessageAsync("marketplace.order-item.process", msg);

        Log.Information("Dispatched marketplace.order-item.process OrderId={OrderId} LineId={LineId} BookId={BookId}",
            msg.CorrelationId, msg.LineId, msg.BookId);
    }

    public async Task CompensateAsync(OrderSagaLine line, CancellationToken ct = default)
    {
        var msg = new MarketplaceOrderlineCompensate(
            CorrelationId: line.OrderId,
            LineId: line.LineId,
            BookId: line.BookId
        );

        await producer.SendMessageAsync("marketplace.order-item.compensate", msg);

        Log.Warning("Dispatched marketplace.order-item.compensate OrderId={OrderId} LineId={LineId} BookId={BookId}",
            msg.CorrelationId, msg.LineId, msg.BookId);
    }
}
