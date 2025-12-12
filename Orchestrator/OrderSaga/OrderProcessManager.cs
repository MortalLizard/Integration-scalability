using Orchestrator.Gateway.DTOs;
using Orchestrator.OrderSaga.Database.Entities;
using Orchestrator.OrderSaga.Database.Repository;
using Orchestrator.OrderSaga.Utils.Enrichers;
using Orchestrator.OrderSaga.Utils.Mappers;
using Serilog;
using Shared;
using Shared.Contracts.OrderBook;

namespace Orchestrator.OrderSaga;

public class OrderProcessManager(Producer producer, IOrderSagaRepository sagaRepository) : IOrderProcessManager
{
    public async Task HandleNewOrderAsync(OrderDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var saga = dto.ToInitialSagaState();
        await sagaRepository.SaveAsync(saga, ct);

        Log.Information("Order saga started. OrderId={OrderId}, BuyerEmail={BuyerEmail}, Items={ItemCount}",
            saga.OrderId, saga.BuyerEmail, saga.LinesExpected);

        // Generate stable lineIds once and reuse for DB + outgoing messages
        var toDispatch = dto.Items
            .Select(orderLine => new { Line = orderLine, LineId = Guid.NewGuid() })
            .ToList();

        // Persist all saga lines in one batch
        var sagaLines = toDispatch
            .Select(x => x.Line.ToSagaLine(saga.OrderId, x.LineId))
            .ToList();

        await sagaRepository.AddLinesAsync(sagaLines, ct);

        // Split + content-based route
        foreach (var x in toDispatch)
        {
            if (x.Line.Marketplace)
            {
                var msg = x.Line.ToMarketplaceOrderlineProcess(saga.OrderId, x.LineId);
                await producer.SendMessageAsync("marketplace.order-item.process", msg);

                Log.Information("Dispatched marketplace.order-item.process OrderId={OrderId} LineId={LineId} BookId={BookId}",
                    msg.CorrelationId, msg.LineId, msg.BookId);
            }
            else
            {
                var msg = x.Line.ToInventoryOrderlineProcess(saga.OrderId, x.LineId);
                await producer.SendMessageAsync("inventory.order-item.process", msg);

                Log.Information("Dispatched inventory.order-item.process OrderId={OrderId} LineId={LineId} BookId={BookId} Quantity={Quantity}",
                    msg.CorrelationId, msg.LineId, msg.BookId, msg.Quantity);
            }
        }

        saga.Status = OrderSagaStatus.WaitingForLineResults;
        saga.Touch();
        await sagaRepository.SaveAsync(saga, ct);

        Log.Information("Saga for OrderId={OrderId} moved to {Phase}. Waiting for line results ({Expected} lines).",
            saga.OrderId, saga.Status, saga.LinesExpected);
    }

    public Task HandleInventoryLineSucceededAsync(InventoryOrderlineProcessed msg, CancellationToken ct = default)
        => HandleLineSucceededAsync(orderId: msg.CorrelationId, lineId: msg.LineId, ct);

    public Task HandleInventoryLineFailedAsync(InventoryOrderlineProcessFailed msg, CancellationToken ct = default)
        => HandleLineFailedAsync(orderId: msg.CorrelationId, lineId: msg.LineId, reason: "Inventory reservation failed", ct);

    public Task HandleMarketplaceLineSucceededAsync(MarketplaceOrderlineProcessed msg, CancellationToken ct = default)
        => HandleLineSucceededAsync(orderId: msg.CorrelationId, lineId: msg.LineId, ct);

    public Task HandleMarketplaceLineFailedAsync(MarketplaceOrderlineProcessFailed msg, CancellationToken ct = default)
        => HandleLineFailedAsync(orderId: msg.CorrelationId, lineId: msg.LineId, reason: "Marketplace reservation failed", ct);

    private async Task HandleLineSucceededAsync(Guid orderId, Guid lineId, CancellationToken ct)
    {
        var saga = await sagaRepository.GetAsync(orderId, ct);
        if (saga is null)
            return;

        // If we are already compensating/failed/completed, we NEVER progress forward.
        // But we DO handle late successes: mark reserved (if pending), then compensate that one line.
        if (saga.Status is OrderSagaStatus.Compensating or OrderSagaStatus.Failed)
        {
            var reservedNow = await sagaRepository.TryMarkLineReservedAsync(orderId, lineId, ct);
            if (reservedNow)
            {
                await CompensateSingleLineAsync(orderId, lineId, ct);
            }
            return;
        }

        if (saga.Status == OrderSagaStatus.Completed)
            return;

        // Normal path: only accept line results in WaitingForLineResults
        if (saga.Status != OrderSagaStatus.WaitingForLineResults)
            return;

        var updated = await sagaRepository.TryMarkLineReservedAsync(orderId, lineId, ct);
        if (!updated)
            return; // duplicate/out-of-order

        // Fail-fast check: if any failed exists, try to start compensating (single winner)
        var failed = await sagaRepository.CountFailedAsync(orderId, ct);
        if (failed > 0)
        {
            await StartCompensationIfWinnerAsync(orderId, "A line failed (detected after success reply)", ct);
            return;
        }

        var reserved = await sagaRepository.CountReservedAsync(orderId, ct);
        if (reserved == saga.LinesExpected)
        {
            // single-winner transition prevents double payment request
            var won = await sagaRepository.TryAdvanceStatusAsync(orderId, OrderSagaStatus.WaitingForLineResults, OrderSagaStatus.PaymentAndReserve, ct);
            if (won)
            {
                Log.Information("All lines reserved. Proceeding to payment. OrderId={OrderId}", orderId);

                // Stub step will be added next (payment.authorize.request)
                saga.PaymentAuthorized = true; // stubbed as true for now
                saga.InventoryReserved = true;
                saga.Status = OrderSagaStatus.InvoiceShipSearch;
                saga.Touch();
                await sagaRepository.SaveAsync(saga, ct);

                // Stub downstream fan-out (shipping/billing/search) will be message-driven as you requested
                await producer.SendMessageAsync("shipping.request", new { correlation_id = orderId });
                await producer.SendMessageAsync("billing.invoice.request", new { correlation_id = orderId });
                await producer.SendMessageAsync("search.update.request", new { correlation_id = orderId });

                saga.Shipped = true;
                saga.Invoiced = true;
                saga.SearchUpdated = true;
                saga.Status = OrderSagaStatus.Completed;
                saga.Touch();
                await sagaRepository.SaveAsync(saga, ct);
            }
        }
    }

    private async Task HandleLineFailedAsync(Guid orderId, Guid lineId, string reason, CancellationToken ct)
    {
        var saga = await sagaRepository.GetAsync(orderId, ct);
        if (saga is null)
            return;

        // If already compensating/failed/completed, ignore failure (compensation already handled or not needed)
        if (saga.Status is OrderSagaStatus.Compensating or OrderSagaStatus.Failed or OrderSagaStatus.Completed)
            return;

        if (saga.Status != OrderSagaStatus.WaitingForLineResults)
            return;

        var updated = await sagaRepository.TryMarkLineFailedAsync(orderId, lineId, reason, ct);
        if (!updated)
            return; // duplicate/out-of-order

        await StartCompensationIfWinnerAsync(orderId, reason, ct);
    }

    private async Task StartCompensationIfWinnerAsync(Guid orderId, string reason, CancellationToken ct)
    {
        // single-winner: only one consumer instance should kick off compensation
        var won = await sagaRepository.TryAdvanceStatusAsync(orderId, OrderSagaStatus.WaitingForLineResults, OrderSagaStatus.Compensating, ct);
        if (!won)
            return;

        var saga = await sagaRepository.GetAsync(orderId, ct);
        if (saga is null)
            return;

        saga.FailureReason = reason;
        saga.Touch();
        await sagaRepository.SaveAsync(saga, ct);

        Log.Warning("Saga entering Compensating. OrderId={OrderId}, Reason={Reason}", orderId, reason);

        // compensate all currently reserved lines
        var reservedLines = await sagaRepository.GetLinesByStatusAsync(orderId, OrderSagaLineStatus.Reserved, ct);
        foreach (var line in reservedLines)
        {
            await CompensateLinePayloadAsync(line, ct);
            await sagaRepository.TryMarkLineCompensationSentAsync(orderId, line.LineId, ct);
        }

        // mark final failed
        saga.Status = OrderSagaStatus.Failed;
        saga.Touch();
        await sagaRepository.SaveAsync(saga, ct);

        Log.Warning("Saga marked Failed after compensation dispatch. OrderId={OrderId}", orderId);
    }

    private async Task CompensateSingleLineAsync(Guid orderId, Guid lineId, CancellationToken ct)
    {
        var lines = await sagaRepository.GetLinesByStatusAsync(orderId, OrderSagaLineStatus.Reserved, ct);
        var line = lines.FirstOrDefault(x => x.LineId == lineId);
        if (line is null)
            return;

        await CompensateLinePayloadAsync(line, ct);
        await sagaRepository.TryMarkLineCompensationSentAsync(orderId, lineId, ct);
    }

    private async Task CompensateLinePayloadAsync(OrderSagaLine line, CancellationToken ct)
    {
        if (line.Marketplace)
        {
            await producer.SendMessageAsync("marketplace.revert.request", new
            {
                correlation_id = line.OrderId,
                line_id = line.LineId,
                book_id = line.BookId,
                price = line.Price
            });
        }
        else
        {
            await producer.SendMessageAsync("inventory.release.request", new
            {
                correlation_id = line.OrderId,
                line_id = line.LineId,
                book_id = line.BookId,
                quantity = line.Quantity,
                price = line.Price
            });
        }
    }
}

