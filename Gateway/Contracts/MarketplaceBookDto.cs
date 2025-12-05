using System.Text.Json.Serialization;
namespace Gateway.Contracts;

public sealed record MarketplaceBookDto
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    [JsonPropertyName("author")]
    public required string Author { get; set; }
    [JsonPropertyName("isbn")]
    public required string Isbn { get; set; }
    [JsonPropertyName("publisher")]
    public required decimal Price { get; set; }
    [JsonPropertyName("publishedDate")]
    public required DateTime PublishedDate { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
