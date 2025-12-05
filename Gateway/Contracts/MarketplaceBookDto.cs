using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace Gateway.Contracts;

public class MarketplaceBookDto
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    [JsonPropertyName("author")]
    public required string Author { get; set; }
    [JsonPropertyName("isbn")]
    public required string Isbn { get; set; }
    [JsonPropertyName("publisher")]
    public decimal Price { get; set; }
    [JsonPropertyName("publishedDate")]
    public DateTime PublishedDate { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }

}
