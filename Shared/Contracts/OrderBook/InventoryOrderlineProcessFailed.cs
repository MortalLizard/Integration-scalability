using System.Text.Json.Serialization;

namespace Shared.Contracts.OrderBook;

public sealed record InventoryOrderlineProcessFailed(
    [property: JsonPropertyName("correlation_id")]
    [property: JsonRequired]
    Guid CorrelationId,

    [property: JsonPropertyName("line_id")]
    [property: JsonRequired]
    Guid LineId,

    [property: JsonPropertyName("book_id")]
    [property: JsonRequired]
    Guid BookId,

    [property: JsonPropertyName("quantity")]
    [property: JsonRequired]
    int Quantity,

    [property: JsonPropertyName("price")]
    [property: JsonRequired]
    decimal Price
);
