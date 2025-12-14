using System.Text.Json.Serialization;

namespace Shared.Contracts.OrderBook.Shipping;

public sealed record ShippingRequest(
    [property: JsonPropertyName("correlation_id")]
    [property: JsonRequired]
    Guid CorrelationId
);

public sealed record ShippingCompleted(
    [property: JsonPropertyName("correlation_id")]
    [property: JsonRequired]
    Guid CorrelationId
);

public sealed record ShippingFailed(
    [property: JsonPropertyName("correlation_id")]
    [property: JsonRequired]
    Guid CorrelationId,

    [property: JsonPropertyName("reason")]
    [property: JsonRequired]
    string Reason
);
