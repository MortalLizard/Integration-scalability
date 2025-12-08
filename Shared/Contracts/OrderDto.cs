using System.Text.Json.Serialization;

namespace Shared.Contracts;

public sealed record OrderDto
{
    [JsonIgnore]
    public string? OrderId { get; set; }
    [JsonPropertyName("buyer_email")]
    public required string BuyerEmail { get; set; }
    [JsonPropertyName("items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]

    public List<OrderlineDto>? Items { get; set; }
    public int TotalItems { get; set; }

}
