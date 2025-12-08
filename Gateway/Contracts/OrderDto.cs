using System.Text.Json.Serialization;

namespace Gateway.Contracts;

public sealed record OrderDto
{
    [JsonIgnore]
    public string? OrderId { get; set; }
    [JsonPropertyName("buyer_email")]
    public required string BuyerEmail { get; set; }
    [JsonPropertyName("items")]
    public required List<OrderlineDto> Items { get; set; }
    public int TotalItems => Items.Sum(i => i.Amount);

}
