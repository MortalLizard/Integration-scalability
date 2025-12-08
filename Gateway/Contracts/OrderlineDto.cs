using System.Text.Json.Serialization;

namespace Gateway.Contracts;

public sealed record OrderlineDto
{
    [JsonPropertyName("book_id")]
    public required string BookId { get; set; }
    [JsonPropertyName("amount")]
    public required int Amount { get; set; }
    [JsonPropertyName("marketplace")]
    public required bool Marketplace { get; set; }
    [JsonIgnore]
    public string? CorrelationId { get; set; }
}
