using Orchestrator.Gateway.DTOs;
using Shared.Contracts.OrderBook;
using Shared.Contracts.OrderBook.Billing;
using Shared.Contracts.OrderBook.Shipping;

namespace Orchestrator.OrderSaga;

public interface IOrderProcessManager
{
    Task HandleNewOrderAsync(OrderDto dto, CancellationToken ct = default);

    Task HandleInventoryLineSucceededAsync(InventoryOrderlineProcessed msg, CancellationToken ct = default);
    Task HandleInventoryLineFailedAsync(InventoryOrderlineProcessFailed msg, CancellationToken ct = default);

    Task HandleMarketplaceLineSucceededAsync(MarketplaceOrderlineProcessed msg, CancellationToken ct = default);
    Task HandleMarketplaceLineFailedAsync(MarketplaceOrderlineProcessFailed msg, CancellationToken ct = default);

    Task HandlePaymentAuthorizedAsync(BillingAuthorized msg, CancellationToken ct = default);
    Task HandlePaymentAuthorizationFailedAsync(BillingAuthorizationFailed msg, CancellationToken ct = default);

    Task HandleBillingInvoicedAsync(BillingInvoiced msg, CancellationToken ct = default);
    Task HandleBillingInvoiceFailedAsync(BillingInvoiceFailed msg, CancellationToken ct = default);

    Task HandleShippingCompletedAsync(ShippingCompleted msg, CancellationToken ct = default);
    Task HandleShippingFailedAsync(ShippingFailed msg, CancellationToken ct = default);
}
