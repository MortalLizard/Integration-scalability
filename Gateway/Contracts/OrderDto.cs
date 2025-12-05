using System.Text.Json.Serialization;

namespace Gateway.Contracts;

public sealed record OrderDto
{
    [JsonIgnore]
    public required string OrderId { get; set; }
    [JsonPropertyName("buyer_email")]
    public required string BuyerEmail { get; set; }
    [JsonPropertyName("items")]
    public required List<OrderlineDto> Items { get; set; }
    [JsonPropertyName("total_amount")]
    public required decimal TotalAmount { get; set; }
}
