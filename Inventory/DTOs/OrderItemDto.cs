using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Inventory.DTOs;

public record OrderItemDto(

    [property: JsonPropertyName("orderId")]
    [property: Required]
    Guid OrderId,

    [property: JsonPropertyName("quantityChange")]
    [property: Required]
    [property: Range(typeof(int), "-999999999", "999999999")]
    int QuantityChange,

    [property: JsonPropertyName("productId")]
    [property: Required]
    Guid ProductId,

    [property: JsonPropertyName("portion")]
    [property: Required]
    [property: Range(0.0, 1.0)]
    decimal Portion
);
