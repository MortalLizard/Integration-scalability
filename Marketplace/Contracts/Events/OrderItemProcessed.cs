using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marketplace.Contracts.Events;

public sealed record OrderItemProcessed(

    [property: JsonPropertyName("correlation_id")]
    [property: Required]
    Guid CorrelationId,

    [property: JsonPropertyName("book_id")]
    [property: Required]
    Guid BookId,

    [property: JsonPropertyName("price")]
    [property: Required]
    decimal Price
);
