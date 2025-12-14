using Orchestrator.Gateway.DTOs;
using Orchestrator.OrderSaga.Database.Entities;
using Orchestrator.OrderSaga.Database.Repository;
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

        var sagaLines = dto.Items
            .Select(orderLine => orderLine.ToSagaLine(saga.OrderId))
            .ToList();

        await sagaRepository.AddLinesAsync(sagaLines, ct);

        foreach (var line in sagaLines)
        {
            if (line.Marketplace)
            {
                var msg = line.ToMarketplaceOrderlineProcess();
                await producer.SendMessageAsync("marketplace.order-item.process", msg);
                Log.Information("Dispatched marketplace.order-item.process OrderId={OrderId} LineId={LineId} BookId={BookId}",
                    msg.CorrelationId, msg.LineId, msg.BookId);
            }
            else
            {
                var msg = line.ToInventoryOrderlineProcess();
                await producer.SendMessageAsync("inventory.order-item.process", msg);
                Log.Information("Dispatched inventory.order-item.process OrderId={OrderId} LineId={LineId} BookId={BookId} Quantity={Quantity}",
                    msg.CorrelationId, msg.LineId, msg.BookId, msg.Quantity);
            }
        }

        await producer.SendMessageAsync("billing.authorize.request", new BillingAuthorizeRequest(saga.OrderId));
        Log.Information("Dispatched billing.authorize.request OrderId={OrderId}", saga.OrderId);

        Log.Information("Saga started. OrderId={OrderId} Status={Status} ExpectedLines={Expected}",
            saga.OrderId, saga.Status, saga.LinesExpected);
    }

    // Inventory replies
    public Task HandleInventoryLineSucceededAsync(InventoryOrderlineProcessed msg, CancellationToken ct = default)
        => HandleLineSucceededAsync(msg.CorrelationId, msg.LineId, ct);

    public Task HandleInventoryLineFailedAsync(InventoryOrderlineProcessFailed msg, CancellationToken ct = default)
        => HandleLineFailedAsync(msg.CorrelationId, msg.LineId, "Inventory reservation failed", ct);

    // Marketplace replies
    public Task HandleMarketplaceLineSucceededAsync(MarketplaceOrderlineProcessed msg, CancellationToken ct = default)
        => HandleLineSucceededAsync(msg.CorrelationId, msg.LineId, ct);

    public Task HandleMarketplaceLineFailedAsync(MarketplaceOrderlineProcessFailed msg, CancellationToken ct = default)
        => HandleLineFailedAsync(msg.CorrelationId, msg.LineId, "Marketplace reservation failed", ct);

    // Billing authorization
    public async Task HandlePaymentAuthorizedAsync(BillingAuthorized msg, CancellationToken ct = default)
    {
        var saga = await sagaRepository.GetAsync(msg.CorrelationId, ct);
        if (saga is null || IsTerminal(saga.Status) || saga.Status == OrderSagaStatus.Compensating)
            return;

        if (saga.Status != OrderSagaStatus.PaymentAndReserve)
            return;

        saga.PaymentAuthorized = true;
        saga.Touch();
        await sagaRepository.SaveAsync(saga, ct);

        Log.Information("Billing authorized. OrderId={OrderId}", msg.CorrelationId);

        await TryProgressAsync(msg.CorrelationId, ct);
    }

    public Task HandlePaymentAuthorizationFailedAsync(BillingAuthorizationFailed msg, CancellationToken ct = default)
        => FailOrderAsync(msg.CorrelationId, $"Payment authorization failed: {msg.Reason}", ct);

    // Shipping
    public async Task HandleShippingCompletedAsync(ShippingCompleted msg, CancellationToken ct = default)
    {
        var saga = await sagaRepository.GetAsync(msg.CorrelationId, ct);
        if (saga is null || IsTerminal(saga.Status) || saga.Status == OrderSagaStatus.Compensating)
            return;

        if (saga.Status != OrderSagaStatus.InvoiceShipSearch)
            return;

        saga.Shipped = true;
        saga.Touch();
        await sagaRepository.SaveAsync(saga, ct);

        Log.Information("Shipping completed. OrderId={OrderId}", msg.CorrelationId);

        await TryProgressAsync(msg.CorrelationId, ct);
    }

    public Task HandleShippingFailedAsync(ShippingFailed msg, CancellationToken ct = default)
        => FailOrderAsync(msg.CorrelationId, $"Shipping failed: {msg.Reason}", ct);

    // Billing invoice
    public async Task HandleBillingInvoicedAsync(BillingInvoiced msg, CancellationToken ct = default)
    {
        var saga = await sagaRepository.GetAsync(msg.CorrelationId, ct);
        if (saga is null || IsTerminal(saga.Status) || saga.Status == OrderSagaStatus.Compensating)
            return;

        if (saga.Status != OrderSagaStatus.InvoiceShipSearch)
            return;

        saga.Invoiced = true;
        saga.Touch();
        await sagaRepository.SaveAsync(saga, ct);

        Log.Information("Billing invoiced. OrderId={OrderId}", msg.CorrelationId);

        await TryProgressAsync(msg.CorrelationId, ct);
    }

    public Task HandleBillingInvoiceFailedAsync(BillingInvoiceFailed msg, CancellationToken ct = default)
        => FailOrderAsync(msg.CorrelationId, $"Billing invoice failed: {msg.Reason}", ct);

    private async Task HandleLineSucceededAsync(Guid orderId, Guid lineId, CancellationToken ct)
    {
        var saga = await sagaRepository.GetAsync(orderId, ct);
        if (saga is null || saga.Status == OrderSagaStatus.Completed)
            return;

        // Late success: reserve then compensate
        if (saga.Status is OrderSagaStatus.Compensating or OrderSagaStatus.Failed)
        {
            var reservedNow = await sagaRepository.TryMarkLineReservedAsync(orderId, lineId, ct);
            if (!reservedNow)
            {
                Log.Warning("Late line success ignored: could not mark Reserved (id mismatch or not Pending). OrderId={OrderId} LineId={LineId} SagaStatus={Status}",
                    orderId, lineId, saga.Status);
                return;
            }

            await CompensateSingleLineAsync(orderId, lineId, ct);
            return;
        }

        if (saga.Status != OrderSagaStatus.PaymentAndReserve)
            return;

        var updated = await sagaRepository.TryMarkLineReservedAsync(orderId, lineId, ct);
        if (!updated)
        {
            Log.Warning("Line success ignored: could not mark Reserved (id mismatch or not Pending). OrderId={OrderId} LineId={LineId} SagaStatus={Status}",
                orderId, lineId, saga.Status);
            return;
        }

        await TryProgressAsync(orderId, ct);
    }

    private async Task HandleLineFailedAsync(Guid orderId, Guid lineId, string reason, CancellationToken ct)
    {
        var saga = await sagaRepository.GetAsync(orderId, ct);
        if (saga is null || saga.Status == OrderSagaStatus.Completed)
            return;

        if (saga.Status is OrderSagaStatus.Compensating or OrderSagaStatus.Failed)
        {
            var late = await sagaRepository.TryMarkLineFailedAsync(orderId, lineId, reason, ct);
            if (!late)
            {
                Log.Warning("Late line failure ignored: could not mark Failed. OrderId={OrderId} LineId={LineId} SagaStatus={Status}",
                    orderId, lineId, saga.Status);
            }
            return;
        }

        var updated = await sagaRepository.TryMarkLineFailedAsync(orderId, lineId, reason, ct);
        if (!updated)
        {
            Log.Warning("Line failure could not be applied (id mismatch or not Pending). OrderId={OrderId} LineId={LineId} SagaStatus={Status}",
                orderId, lineId, saga.Status);
            return;
        }

        await FailOrderAsync(orderId, reason, ct);
    }

    private async Task TryProgressAsync(Guid orderId, CancellationToken ct)
    {
        var saga = await sagaRepository.GetAsync(orderId, ct);
        if (saga is null || IsTerminal(saga.Status) || saga.Status == OrderSagaStatus.Compensating)
            return;

        if (saga.Status == OrderSagaStatus.PaymentAndReserve)
        {
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

            var won = await sagaRepository.TryAdvanceStatusAsync(orderId, OrderSagaStatus.PaymentAndReserve, OrderSagaStatus.InvoiceShipSearch, ct);
            if (!won)
                return;

            await producer.SendMessageAsync("shipping.request", new ShippingRequest(orderId));
            await producer.SendMessageAsync("billing.invoice.request", new BillingInvoiceRequest(orderId));
            await producer.SendMessageAsync("search.update.request", new { correlation_id = orderId });

            saga.SearchUpdated = true;
            saga.Status = OrderSagaStatus.InvoiceShipSearch;
            saga.Touch();
            await sagaRepository.SaveAsync(saga, ct);

            Log.Information("Advanced to InvoiceShipSearch. OrderId={OrderId}", orderId);
            return;
        }

        if (saga.Status == OrderSagaStatus.InvoiceShipSearch)
        {
            if (saga.Shipped && saga.Invoiced && saga.SearchUpdated)
            {
                saga.Status = OrderSagaStatus.Completed;
                saga.Touch();
                await sagaRepository.SaveAsync(saga, ct);

                Log.Information("Saga completed. OrderId={OrderId}", orderId);
            }
        }
    }

    private async Task FailOrderAsync(Guid orderId, string reason, CancellationToken ct)
    {
        var won =
            await sagaRepository.TryAdvanceStatusAsync(orderId, OrderSagaStatus.PaymentAndReserve, OrderSagaStatus.Compensating, ct)
            || await sagaRepository.TryAdvanceStatusAsync(orderId, OrderSagaStatus.InvoiceShipSearch, OrderSagaStatus.Compensating, ct)
            || await sagaRepository.TryAdvanceStatusAsync(orderId, OrderSagaStatus.NewOrderReceived, OrderSagaStatus.Compensating, ct);

        if (!won)
            return;

        var saga = await sagaRepository.GetAsync(orderId, ct);
        if (saga is null)
            return;

        saga.FailureReason = reason;
        saga.Status = OrderSagaStatus.Compensating;
        saga.Touch();
        await sagaRepository.SaveAsync(saga, ct);

        // Validation logs
        var pendingLines = await sagaRepository.GetLinesByStatusAsync(orderId, OrderSagaLineStatus.Pending, ct);
        var reservedLines = await sagaRepository.GetLinesByStatusAsync(orderId, OrderSagaLineStatus.Reserved, ct);
        var sentLines = await sagaRepository.GetLinesByStatusAsync(orderId, OrderSagaLineStatus.CompensationSent, ct);

        Log.Warning(
            "Order failing; starting compensation. OrderId={OrderId} Reason={Reason} PendingLines={Pending} ReservedLines={Reserved} CompensationSentLines={Sent}",
            orderId, reason, pendingLines.Count, reservedLines.Count, sentLines.Count);

        foreach (var line in reservedLines)
        {
            var marked = await sagaRepository.TryMarkLineCompensationSentAsync(orderId, line.LineId, ct);
            if (!marked)
                continue;

            await CompensateLinePayloadAsync(line, ct);
        }

        saga.Status = OrderSagaStatus.Failed;
        saga.Touch();
        await sagaRepository.SaveAsync(saga, ct);

        Log.Warning("Saga marked Failed. OrderId={OrderId}", orderId);
    }

    private static bool IsTerminal(OrderSagaStatus status)
        => status is OrderSagaStatus.Completed or OrderSagaStatus.Failed;

    private async Task CompensateSingleLineAsync(Guid orderId, Guid lineId, CancellationToken ct)
    {
        var reservedLines = await sagaRepository.GetLinesByStatusAsync(orderId, OrderSagaLineStatus.Reserved, ct);
        var line = reservedLines.FirstOrDefault(x => x.LineId == lineId);
        if (line is null)
            return;

        var marked = await sagaRepository.TryMarkLineCompensationSentAsync(orderId, lineId, ct);
        if (!marked)
            return;

        await CompensateLinePayloadAsync(line, ct);
    }

    private async Task CompensateLinePayloadAsync(OrderSagaLine line, CancellationToken ct)
    {
        if (line.Marketplace)
        {
            await producer.SendMessageAsync("marketplace.order-item.compensate", new MarketplaceOrderlineCompensate(
                CorrelationId: line.OrderId,
                LineId: line.LineId,
                BookId: line.BookId
            ));
        }
        else
        {
            if (line.Quantity is null)
                return;

            await producer.SendMessageAsync("inventory.order-item.compensate", new InventoryOrderlineCompensate(
                CorrelationId: line.OrderId,
                LineId: line.LineId,
                BookId: line.BookId,
                Quantity: line.Quantity.Value
            ));
        }
    }
}
