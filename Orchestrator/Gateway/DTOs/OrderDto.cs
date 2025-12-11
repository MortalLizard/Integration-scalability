using System.Text.Json.Serialization;

namespace Orchestrator.Gateway.DTOs;

public sealed record OrderDto
{
    [JsonPropertyName("buyer_email")]
    public required string BuyerEmail { get; set; }
    [JsonPropertyName("items")]
    public required List<OrderlineDto> Items { get; set; }
}
