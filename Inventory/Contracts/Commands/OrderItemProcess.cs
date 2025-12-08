using System.Text.Json.Serialization;

namespace Inventory.Contracts.Commands;

public sealed record OrderItemProcess(

    [property: JsonPropertyName("correlation_id")]
    [property: JsonRequired]
    Guid CorrelationId,

    [property: JsonPropertyName("quantity")]
    [property: JsonRequired]
    int Quantity,

    [property: JsonPropertyName("book_id")]
    [property: JsonRequired]
    Guid BookId
);
