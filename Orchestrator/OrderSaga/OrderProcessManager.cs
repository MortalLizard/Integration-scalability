using Orchestrator.Gateway.DTOs;
using Orchestrator.OrderSaga.Activities;
using Orchestrator.OrderSaga.Database.Entities;
using Orchestrator.OrderSaga.Database.Repository;
using Orchestrator.OrderSaga.Utils.Mappers;
using Serilog;
using Shared.Contracts.OrderBook;
using Shared.Contracts.OrderBook.Billing;
using Shared.Contracts.OrderBook.Shipping;

namespace Orchestrator.OrderSaga;

public class OrderProcessManager(IOrderSagaRepository sagaRepository, IInventoryOrderLineActivity inventoryLineActivity, IMarketplaceOrderLineActivity marketplaceLineActivity, IBillingAuthorizeActivity billingAuthorizeActivity, IShippingActivity shippingActivity, IBillingInvoiceActivity billingInvoiceActivity, ISearchUpdateActivity searchUpdateActivity) : IOrderProcessManager
{
    public async Task HandleNewOrderAsync(OrderDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var saga = dto.ToInitialSagaState();
        var sagaLines = dto.Items.Select(orderLine => orderLine.ToSagaLine(saga.OrderId)).ToList();

        await sagaRepository.CreateSagaWithLinesAsync(saga, sagaLines, ct);

        foreach (var line in sagaLines)
        {
            if (line.Marketplace)
                await marketplaceLineActivity.ExecuteAsync(line, ct);
            else
                await inventoryLineActivity.ExecuteAsync(line, ct);
        }

        await billingAuthorizeActivity.ExecuteAsync(saga.OrderId, ct);

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

    // Billing authorization replies
    public async Task HandlePaymentAuthorizedAsync(BillingAuthorized msg, CancellationToken ct = default)
    {
        var saga = await sagaRepository.GetSagaAsync(msg.CorrelationId, ct);
        if (saga is null || !saga.CanProcessReplies)
            return;

        var updated = await sagaRepository.TrySetPaymentAuthorizedAsync(msg.CorrelationId, ct);
        if (!updated)
            return;

        Log.Information("Billing authorized. OrderId={OrderId}", msg.CorrelationId);
        await TryProgressAsync(msg.CorrelationId, ct);
    }

    public Task HandlePaymentAuthorizationFailedAsync(BillingAuthorizationFailed msg, CancellationToken ct = default)
        => FailOrderAsync(msg.CorrelationId, $"Payment authorization failed: {msg.Reason}", ct);

    // Shipping replies
    public async Task HandleShippingCompletedAsync(ShippingCompleted msg, CancellationToken ct = default)
    {
        var saga = await sagaRepository.GetSagaAsync(msg.CorrelationId, ct);
        if (saga is null || !saga.CanProcessReplies)
            return;

        var updated = await sagaRepository.TrySetShippedAsync(msg.CorrelationId, ct);
        if (!updated)
            return;

        Log.Information("Shipping completed. OrderId={OrderId}", msg.CorrelationId);
        await TryProgressAsync(msg.CorrelationId, ct);
    }

    public Task HandleShippingFailedAsync(ShippingFailed msg, CancellationToken ct = default)
        => FailOrderAsync(msg.CorrelationId, $"Shipping failed: {msg.Reason}", ct);

    // Billing invoice replies
    public async Task HandleBillingInvoicedAsync(BillingInvoiced msg, CancellationToken ct = default)
    {
        var saga = await sagaRepository.GetSagaAsync(msg.CorrelationId, ct);
        if (saga is null || !saga.CanProcessReplies)
            return;

        bool updated = await sagaRepository.TrySetInvoicedAsync(msg.CorrelationId, ct);
        if (!updated)
            return;

        Log.Information("Billing invoiced. OrderId={OrderId}", msg.CorrelationId);
        await TryProgressAsync(msg.CorrelationId, ct);
    }

    public Task HandleBillingInvoiceFailedAsync(BillingInvoiceFailed msg, CancellationToken ct = default)
        => FailOrderAsync(msg.CorrelationId, $"Billing invoice failed: {msg.Reason}", ct);

    private async Task HandleLineSucceededAsync(Guid orderId, Guid lineId, CancellationToken ct)
    {
        var saga = await sagaRepository.GetSagaAsync(orderId, ct);
        if (saga is null || saga.IsFinished)
            return;

        if (saga.IsInFailureFlow)
        {
            bool reservedNow = await sagaRepository.TryMarkLineReservedAsync(orderId, lineId, ct);
            if (!reservedNow)
            {
                Log.Warning("Late line success ignored: could not mark Reserved. OrderId={OrderId} LineId={LineId} SagaStatus={Status}",
                    orderId, lineId, saga.Status);
                return;
            }

            await CompensateSingleLineAsync(orderId, lineId, ct);
            return;
        }

        if (saga.Status != OrderSagaStatus.PaymentAndReserve)
            return;

        bool updated = await sagaRepository.TrySetLineReservationSuccessAsync(orderId, lineId, ct);
        if (!updated)
        {
            Log.Warning("Line success ignored: could not reserve line. OrderId={OrderId} LineId={LineId} SagaStatus={Status}",
                orderId, lineId, saga.Status);
            return;
        }

        await TryProgressAsync(orderId, ct);
    }

    private async Task HandleLineFailedAsync(Guid orderId, Guid lineId, string reason, CancellationToken ct)
    {
        var saga = await sagaRepository.GetSagaAsync(orderId, ct);
        if (saga is null || saga.IsFinished)
            return;

        // Late fail: just record it
        if (saga.IsInFailureFlow)
        {
            bool late = await sagaRepository.TryMarkLineFailedAsync(orderId, lineId, reason, ct);
            if (!late)
            {
                Log.Warning("Late line failure ignored: could not mark Failed. OrderId={OrderId} LineId={LineId} SagaStatus={Status}",
                    orderId, lineId, saga.Status);
            }
            return;
        }

        bool updated = await sagaRepository.TrySetLineReservationFailureAsync(orderId, lineId, reason, ct);
        if (!updated)
        {
            Log.Warning("Line failure could not be applied. OrderId={OrderId} LineId={LineId} SagaStatus={Status}",
                orderId, lineId, saga.Status);
            return;
        }

        await FailOrderAsync(orderId, reason, ct);
    }

    private async Task TryProgressAsync(Guid orderId, CancellationToken ct)
    {
        var saga = await sagaRepository.GetSagaAsync(orderId, ct);
        if (saga is null || saga.IsFinished || saga.IsCompensating)
            return;

        switch (saga.Status)
        {
            case OrderSagaStatus.PaymentAndReserve:
            {
                var advanced = await sagaRepository.TryAdvanceToInvoiceShipSearchAsync(orderId, ct);
                if (!advanced)
                    return;

                await shippingActivity.ExecuteAsync(orderId, ct);
                await billingInvoiceActivity.ExecuteAsync(orderId, ct);
                await searchUpdateActivity.ExecuteAsync(orderId, ct);

                // OBS TEMPORARY: SearchUpdated markeres med det samme som true, fordi der endnu ikke er implementeret async reply fra Search.
                await sagaRepository.TrySetSearchUpdatedAsync(orderId, ct);

                Log.Information("Advanced to InvoiceShipSearch. OrderId={OrderId}", orderId);
                return;
            }
            case OrderSagaStatus.InvoiceShipSearch:
            {
                bool completed = await sagaRepository.TryCompleteAsync(orderId, ct);
                if (completed)
                    Log.Information("Saga completed. OrderId={OrderId}", orderId);
                break;
            }
        }
    }

    private async Task FailOrderAsync(Guid orderId, string reason, CancellationToken ct)
    {
        bool won =
            await sagaRepository.TryTransitionSagaStatusAsync(orderId, OrderSagaStatus.PaymentAndReserve, OrderSagaStatus.Compensating, ct)
            || await sagaRepository.TryTransitionSagaStatusAsync(orderId, OrderSagaStatus.InvoiceShipSearch, OrderSagaStatus.Compensating, ct)
            || await sagaRepository.TryTransitionSagaStatusAsync(orderId, OrderSagaStatus.NewOrderReceived, OrderSagaStatus.Compensating, ct);

        if (!won)
            return;

        await sagaRepository.TrySetFailureReasonAsync(orderId, reason, ct);

        var saga = await sagaRepository.GetSagaAsync(orderId, ct);
        if (saga is null)
            return;

        if (saga.SearchUpdated)     await searchUpdateActivity.CompensateAsync(orderId, ct);
        if (saga.Invoiced)          await billingInvoiceActivity.CompensateAsync(orderId, ct);
        if (saga.Shipped)           await shippingActivity.CompensateAsync(orderId, ct);
        if (saga.PaymentAuthorized) await billingAuthorizeActivity.CompensateAsync(orderId, ct);

        // Cannot check on InventoryReserved status since some order lines might need compensation even if the overall reservation failed
        var reservedLines = await sagaRepository.GetLinesByStatusAsync(orderId, OrderSagaLineStatus.Reserved, ct);
        foreach (var line in reservedLines)
        {
            bool marked = await sagaRepository.TryMarkLineCompensationSentAsync(orderId, line.LineId, ct);
            if (!marked) continue;

            await CompensateLineAsync(line, ct);
        }

        await sagaRepository.TryTransitionSagaStatusAsync(orderId, OrderSagaStatus.Compensating, OrderSagaStatus.Failed, ct);
        Log.Warning("Saga marked Failed. OrderId={OrderId}", orderId);
    }

    private async Task CompensateSingleLineAsync(Guid orderId, Guid lineId, CancellationToken ct)
    {
        var reservedLines = await sagaRepository.GetLinesByStatusAsync(orderId, OrderSagaLineStatus.Reserved, ct);
        var line = reservedLines.FirstOrDefault(x => x.LineId == lineId);
        if (line is null) return;

        bool marked = await sagaRepository.TryMarkLineCompensationSentAsync(orderId, lineId, ct);
        if (!marked) return;

        await CompensateLineAsync(line, ct);
    }

    private Task CompensateLineAsync(OrderSagaLine line, CancellationToken ct)
        => line.Marketplace
            ? marketplaceLineActivity.CompensateAsync(line, ct)
            : inventoryLineActivity.CompensateAsync(line, ct);
}
