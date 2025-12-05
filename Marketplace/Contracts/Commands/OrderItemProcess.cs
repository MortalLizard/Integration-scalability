using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marketplace.Contracts.Commands;

public sealed record OrderItemProcess(

    [property: JsonPropertyName("orderId")]
    [property: Required]
    Guid OrderId,

    [property: JsonPropertyName("productId")]
    [property: Required]
    Guid ProductId,

    [property: JsonPropertyName("portion")]
    [property: Required]
    [property: Range(0.0, 1.0)]
    decimal Portion
);
