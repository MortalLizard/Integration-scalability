using System.Text.Json.Serialization;

namespace Inventory.Contracts.Events;

public sealed record OrderItemProcessed(

    [property: JsonPropertyName("correlation_id")]
    [property: JsonRequired]
    Guid CorrelationId,

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
