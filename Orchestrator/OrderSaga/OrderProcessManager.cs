using Orchestrator.Gateway.DTOs;
using Orchestrator.OrderSaga.Database.Entities;
using Orchestrator.OrderSaga.Database.Repository;
using Orchestrator.OrderSaga.Utils.Enrichers;
using Orchestrator.OrderSaga.Utils.Mappers;
using Serilog;
using Shared;
using Shared.Contracts.OrderBook;
using Shared.Contracts.OrderBook.Billing;
using Shared.Contracts.OrderBook.Shipping;

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

        // Dispatch ALL line reservations
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

        // Start billing authorization SIMULTANEOUSLY with reservations
        await producer.SendMessageAsync("billing.authorize.request", new BillingAuthorizeRequest(saga.OrderId));
        Log.Information("Dispatched billing.authorize.request OrderId={OrderId}", saga.OrderId);

        // Combined wait: payment authorization + all line results
        saga.Status = OrderSagaStatus.PaymentAndReserve;
        saga.Touch();
        await sagaRepository.SaveAsync(saga, ct);

        Log.Information("Saga for OrderId={OrderId} moved to {Phase}. Waiting for billing auth + line results ({Expected} lines).",
            saga.OrderId, saga.Status, saga.LinesExpected);
    }

    // Inventory replies
    public Task HandleInventoryLineSucceededAsync(InventoryOrderlineProcessed msg, CancellationToken ct = default)
        => HandleLineSucceededAsync(orderId: msg.CorrelationId, lineId: msg.LineId, ct);

    public Task HandleInventoryLineFailedAsync(InventoryOrderlineProcessFailed msg, CancellationToken ct = default)
        => HandleLineFailedAsync(orderId: msg.CorrelationId, lineId: msg.LineId, reason: "Inventory reservation failed", ct);

    // Marketplace replies
    public Task HandleMarketplaceLineSucceededAsync(MarketplaceOrderlineProcessed msg, CancellationToken ct = default)
        => HandleLineSucceededAsync(orderId: msg.CorrelationId, lineId: msg.LineId, ct);

    public Task HandleMarketplaceLineFailedAsync(MarketplaceOrderlineProcessFailed msg, CancellationToken ct = default)
        => HandleLineFailedAsync(orderId: msg.CorrelationId, lineId: msg.LineId, reason: "Marketplace reservation failed", ct);

    // Billing authorization replies (your "payment")
    public async Task HandlePaymentAuthorizedAsync(BillingAuthorized msg, CancellationToken ct = default)
    {
        var saga = await sagaRepository.GetAsync(msg.CorrelationId, ct);
        if (saga is null)
            return;

        if (saga.Status is OrderSagaStatus.Compensating or OrderSagaStatus.Failed or OrderSagaStatus.Completed)
            return;

        // Authorization can arrive while we're in the combined phase
        if (saga.Status != OrderSagaStatus.PaymentAndReserve)
            return;

        saga.PaymentAuthorized = true;
        saga.Touch();
        await sagaRepository.SaveAsync(saga, ct);

        Log.Information("Billing authorized. OrderId={OrderId}", msg.CorrelationId);

        await TryAdvanceAfterPaymentAndReserveAsync(msg.CorrelationId, ct);
    }

    public Task HandlePaymentAuthorizationFailedAsync(BillingAuthorizationFailed msg, CancellationToken ct = default)
        => StartCompensationIfWinnerAsync(msg.CorrelationId, $"Payment authorization failed: {msg.Reason}", ct);

    // Shipping replies
    public async Task HandleShippingCompletedAsync(ShippingCompleted msg, CancellationToken ct = default)
    {
        var saga = await sagaRepository.GetAsync(msg.CorrelationId, ct);
        if (saga is null)
            return;

        if (saga.Status is OrderSagaStatus.Compensating or OrderSagaStatus.Failed or OrderSagaStatus.Completed)
            return;

        if (saga.Status != OrderSagaStatus.InvoiceShipSearch)
            return;

        saga.Shipped = true;
        saga.Touch();
        await sagaRepository.SaveAsync(saga, ct);

        Log.Information("Shipping completed. OrderId={OrderId}", msg.CorrelationId);

        await TryCompleteAsync(msg.CorrelationId, ct);
    }

    public Task HandleShippingFailedAsync(ShippingFailed msg, CancellationToken ct = default)
        => StartCompensationIfWinnerAsync(msg.CorrelationId, $"Shipping failed: {msg.Reason}", ct);

    // Billing invoice replies
    public async Task HandleBillingInvoicedAsync(BillingInvoiced msg, CancellationToken ct = default)
    {
        var saga = await sagaRepository.GetAsync(msg.CorrelationId, ct);
        if (saga is null)
            return;

        if (saga.Status is OrderSagaStatus.Compensating or OrderSagaStatus.Failed or OrderSagaStatus.Completed)
            return;

        if (saga.Status != OrderSagaStatus.InvoiceShipSearch)
            return;

        saga.Invoiced = true;
        saga.Touch();
        await sagaRepository.SaveAsync(saga, ct);

        Log.Information("Billing invoiced. OrderId={OrderId}", msg.CorrelationId);

        await TryCompleteAsync(msg.CorrelationId, ct);
    }

    public Task HandleBillingInvoiceFailedAsync(BillingInvoiceFailed msg, CancellationToken ct = default)
        => StartCompensationIfWinnerAsync(msg.CorrelationId, $"Billing invoice failed: {msg.Reason}", ct);

    private async Task HandleLineSucceededAsync(Guid orderId, Guid lineId, CancellationToken ct)
    {
        var saga = await sagaRepository.GetAsync(orderId, ct);
        if (saga is null)
            return;

        // If we are already compensating/failed, we never progress forward.
        // But we DO handle late successes: mark reserved (if pending), then compensate that one line.
        if (saga.Status is OrderSagaStatus.Compensating or OrderSagaStatus.Failed)
        {
            var reservedNow = await sagaRepository.TryMarkLineReservedAsync(orderId, lineId, ct);
            if (reservedNow)
                await CompensateSingleLineAsync(orderId, lineId, ct);

            return;
        }

        if (saga.Status == OrderSagaStatus.Completed)
            return;

        // Accept line results in the combined phase
        if (saga.Status != OrderSagaStatus.PaymentAndReserve)
            return;

        var updated = await sagaRepository.TryMarkLineReservedAsync(orderId, lineId, ct);
        if (!updated)
            return; // duplicate/out-of-order

        // If any failed already exists, start compensation (single winner)
        var failed = await sagaRepository.CountFailedAsync(orderId, ct);
        if (failed > 0)
        {
            await StartCompensationIfWinnerAsync(orderId, "A line failed (detected after success reply)", ct);
            return;
        }

        await TryAdvanceAfterPaymentAndReserveAsync(orderId, ct);
    }

    private async Task HandleLineFailedAsync(Guid orderId, Guid lineId, string reason, CancellationToken ct)
    {
        var saga = await sagaRepository.GetAsync(orderId, ct);
        if (saga is null)
            return;

        if (saga.Status is OrderSagaStatus.Compensating or OrderSagaStatus.Failed or OrderSagaStatus.Completed)
            return;

        // Accept failures in the combined phase
        if (saga.Status != OrderSagaStatus.PaymentAndReserve)
            return;

        var updated = await sagaRepository.TryMarkLineFailedAsync(orderId, lineId, reason, ct);
        if (!updated)
            return; // duplicate/out-of-order

        await StartCompensationIfWinnerAsync(orderId, reason, ct);
    }

    private async Task TryAdvanceAfterPaymentAndReserveAsync(Guid orderId, CancellationToken ct)
    {
        var saga = await sagaRepository.GetAsync(orderId, ct);
        if (saga is null)
            return;

        if (saga.Status != OrderSagaStatus.PaymentAndReserve)
            return;

        // Do not proceed if any line failed
        var failed = await sagaRepository.CountFailedAsync(orderId, ct);
        if (failed > 0)
            return;

        var reserved = await sagaRepository.CountReservedAsync(orderId, ct);
        var allLinesReserved = reserved == saga.LinesExpected;

        if (allLinesReserved)
            saga.InventoryReserved = true;

        if (!allLinesReserved || !saga.PaymentAuthorized)
        {
            saga.Touch();
            await sagaRepository.SaveAsync(saga, ct);
            return;
        }

        // single-winner: proceed to downstream exactly once
        var won = await sagaRepository.TryAdvanceStatusAsync(orderId, OrderSagaStatus.PaymentAndReserve, OrderSagaStatus.InvoiceShipSearch, ct);
        if (!won)
            return;

        saga.Status = OrderSagaStatus.InvoiceShipSearch;
        saga.Touch();
        await sagaRepository.SaveAsync(saga, ct);

        Log.Information("Payment+Reserve complete. Proceeding to invoice/ship/search. OrderId={OrderId}", orderId);

        await producer.SendMessageAsync("shipping.request", new ShippingRequest(orderId));
        await producer.SendMessageAsync("billing.invoice.request", new BillingInvoiceRequest(orderId));

        // You said search is still stubbed; keep it immediate for now
        await producer.SendMessageAsync("search.update.request", new { correlation_id = orderId });

        saga.SearchUpdated = true;
        saga.Touch();
        await sagaRepository.SaveAsync(saga, ct);

        await TryCompleteAsync(orderId, ct);
    }

    private async Task TryCompleteAsync(Guid orderId, CancellationToken ct)
    {
        var saga = await sagaRepository.GetAsync(orderId, ct);
        if (saga is null)
            return;

        if (saga.Status != OrderSagaStatus.InvoiceShipSearch)
            return;

        if (saga.PaymentAuthorized && saga.InventoryReserved && saga.Shipped && saga.Invoiced && saga.SearchUpdated)
        {
            saga.Status = OrderSagaStatus.Completed;
            saga.Touch();
            await sagaRepository.SaveAsync(saga, ct);

            Log.Information("Saga completed. OrderId={OrderId}", orderId);
        }
    }

    private async Task StartCompensationIfWinnerAsync(Guid orderId, string reason, CancellationToken ct)
    {
        // single-winner: allow compensation to start from either phase that can fail
        var won =
            await sagaRepository.TryAdvanceStatusAsync(orderId, OrderSagaStatus.PaymentAndReserve, OrderSagaStatus.Compensating, ct)
            || await sagaRepository.TryAdvanceStatusAsync(orderId, OrderSagaStatus.WaitingForLineResults, OrderSagaStatus.Compensating, ct);

        if (!won)
            return;

        var saga = await sagaRepository.GetAsync(orderId, ct);
        if (saga is null)
            return;

        saga.FailureReason = reason;
        saga.Status = OrderSagaStatus.Compensating;
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

        // NOTE: if BillingAuthorized can happen before a line fails, you may want a "billing.void.request" here later.

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
            await producer.SendMessageAsync("marketplace.revert.request", new MarketplaceOrderlineCompensate(
                CorrelationId: line.OrderId,
                LineId: line.LineId,
                BookId: line.BookId
            ));
        }
        else
        {
            await producer.SendMessageAsync("inventory.release.request", new InventoryOrderlineCompensate(
                CorrelationId: line.OrderId,
                LineId: line.LineId,
                BookId: line.BookId,
                Quantity: line.Quantity.Value
            ));
        }
    }
}
