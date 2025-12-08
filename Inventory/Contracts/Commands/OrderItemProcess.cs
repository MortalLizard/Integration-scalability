using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Inventory.Contracts.Commands;

public sealed record OrderItemProcess(

    [property: JsonPropertyName("correlation_id")]
    [property: Required]
    Guid CorrelationId,

    [property: JsonPropertyName("quantity")]
    [property: Required]
    [property: Range(typeof(int), "1", "999999999")]
    int Quantity,

    [property: JsonPropertyName("book_id")]
    [property: Required]
    Guid BookId
);
