using System.Text.Json.Serialization;

namespace Shared.Contracts;

public sealed record OrderlineDto
{
    [JsonPropertyName("book_id")]
    public required string BookId { get; set; }
    [JsonPropertyName("quantity")]
    public required int Quantity { get; set; }
    [JsonPropertyName("marketplace")]
    public required bool Marketplace { get; set; }
    [JsonIgnore]
    public string? CorrelationId { get; set; }
}
