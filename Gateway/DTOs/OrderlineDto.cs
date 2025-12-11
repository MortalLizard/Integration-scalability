using System.Text.Json.Serialization;

namespace Gateway.DTOs;

public sealed record OrderlineDto
{
    [JsonPropertyName("book_id")]
    public required Guid BookId { get; set; }
    [JsonPropertyName("correlation_id")]
    public Guid? CorrelationId { get; set; }
    [JsonPropertyName("price")]
    public required decimal Price { get; set; }

    [JsonPropertyName("quantity")]
    public int? Quantity { get; set; }

    [JsonPropertyName("marketplace")]
    public required bool Marketplace { get; set; }
}
