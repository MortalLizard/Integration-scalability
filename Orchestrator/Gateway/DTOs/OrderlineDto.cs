using System.Text.Json.Serialization;

namespace Orchestrator.Gateway.DTOs;

public sealed record OrderlineDto
{
    [JsonPropertyName("book_id")]
    public required Guid BookId { get; set; }
    [JsonPropertyName("price")]
    public required decimal Price { get; set; }
    [JsonPropertyName("quantity")]
    public int? Quantity { get; set; }
    [JsonPropertyName("marketplace")]
    public required bool Marketplace { get; set; }
}
