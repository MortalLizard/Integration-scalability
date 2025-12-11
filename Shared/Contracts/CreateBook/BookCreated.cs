using System.Text.Json.Serialization;

namespace Shared.Contracts.CreateBook;

public sealed record BookCreated(
    [property: JsonPropertyName("id")]
    [property: JsonRequired]
    Guid Id,

    [property: JsonPropertyName("title")]
    [property: JsonRequired]
    string Title,

    [property: JsonPropertyName("author")]
    [property: JsonRequired]
    string Author,

    [property: JsonPropertyName("isbn")]
    [property: JsonRequired]
    string Isbn,

    [property: JsonPropertyName("price")]
    [property: JsonRequired]
    decimal Price,

    [property: JsonPropertyName("published_date")]
    [property: JsonRequired]
    DateTime PublishedDate,

    [property: JsonPropertyName("description")]
    string? Description
);
