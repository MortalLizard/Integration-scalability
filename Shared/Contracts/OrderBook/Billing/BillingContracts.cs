using System.Text.Json.Serialization;

namespace Shared.Contracts.OrderBook.Billing;

// Billing contracts for authorization of payment
public sealed record BillingAuthorizeRequest(
    [property: JsonPropertyName("correlation_id")]
    [property: JsonRequired]
    Guid CorrelationId
);

public sealed record BillingAuthorized(
    [property: JsonPropertyName("correlation_id")]
    [property: JsonRequired]
    Guid CorrelationId
);

public sealed record BillingAuthorizationFailed(
    [property: JsonPropertyName("correlation_id")]
    [property: JsonRequired]
    Guid CorrelationId,

    [property: JsonPropertyName("reason")]
    [property: JsonRequired]
    string Reason
);

// Billing contracts for invoice
public sealed record BillingInvoiceRequest(
    [property: JsonPropertyName("correlation_id")]
    [property: JsonRequired]
    Guid CorrelationId
);

public sealed record BillingInvoiced(
    [property: JsonPropertyName("correlation_id")]
    [property: JsonRequired]
    Guid CorrelationId
);

public sealed record BillingInvoiceFailed(
    [property: JsonPropertyName("correlation_id")]
    [property: JsonRequired]
    Guid CorrelationId,

    [property: JsonPropertyName("reason")]
    [property: JsonRequired]
    string Reason
);
