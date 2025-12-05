using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marketplace.Contracts.Events;

public sealed record OrderItemProcessed(

    [property: JsonPropertyName("orderId")]
    [property: Required]
    Guid OrderId,

    [property: JsonPropertyName("email")]
    [property: Required]
    string Email,

    [property: JsonPropertyName("productId")]
    [property: Required]
    Guid ProductId,

    [property: JsonPropertyName("price")]
    [property: Required]
    decimal Price,

    [property: JsonPropertyName("portion")]
    [property: Required]
    [property: Range(0.0, 1.0)]
    decimal Portion,

    [property: JsonPropertyName("timestamp")]
    [property: Required]
    DateTime Timestamp
);
