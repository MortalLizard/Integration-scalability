using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Inventory.Contracts.Commands;

public sealed record OrderItemProcess(

    [property: JsonPropertyName("correlation_id")]
    [property: Required]
    Guid CorrelationId,

    [property: JsonPropertyName("email")]
    [property: Required]
    string Email,

    [property: JsonPropertyName("quantityChange")]
    [property: Required]
    [property: Range(typeof(int), "1", "999999999")]
    int Quantity,

    [property: JsonPropertyName("book_id")]
    [property: Required]
    Guid BookId,

    [property: JsonPropertyName("portion")]
    [property: Required]
    [property: Range(0.0, 1.0)]
    decimal Portion
);
