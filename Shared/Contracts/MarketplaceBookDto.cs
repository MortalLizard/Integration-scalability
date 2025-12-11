using System.Text.Json.Serialization;
namespace Shared.Contracts;

public sealed record MarketplaceBookDto
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    [JsonPropertyName("author")]
    public required string Author { get; set; }
    [JsonPropertyName("isbn")]
    public required string Isbn { get; set; }
    [JsonPropertyName("price")]
    public required decimal Price { get; set; }
    [JsonPropertyName("published_date")]
    public required DateTime PublishedDate { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("seller_id")]
    public string? SellerId { get; set; }
}
