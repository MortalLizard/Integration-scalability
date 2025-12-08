using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Inventory.Contracts.Events;

public sealed record OrderItemProcessed(

    [property: JsonPropertyName("correlation_id")]
    [property: Required]
    Guid CorrelationId,

    [property: JsonPropertyName("book_id")]
    [property: Required]
    Guid BookId,

    [property: JsonPropertyName("quantity")]
    [property: Required]
    [property: Range(typeof(int), "1", "999999999")]
    int Quantity,

    [property: JsonPropertyName("price")]
    [property: Required]
    decimal Price
);
