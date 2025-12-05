using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marketplace.Contracts.Events;

public sealed record BookCreated(
    [property: JsonPropertyName("id")]
    [property: Required]
    Guid Id,

    [property: JsonPropertyName("title")]
    [property: Required]
    string Title,

    [property: JsonPropertyName("author")]
    [property: Required]
    string Author,

    [property: JsonPropertyName("isbn")]
    [property: Required]
    string Isbn,

    [property: JsonPropertyName("price")]
    [property: Range(typeof(decimal), "0", "999999999")]
    [property: Required]
    decimal Price,

    [property: JsonPropertyName("publishedDate")]
    [property: Required]
    DateTime PublishedDate,

    [property: JsonPropertyName("description")]
    string? Description,

    [property: JsonPropertyName("isActive")]
    [property: Required]
    bool IsActive,

    [property: JsonPropertyName("timestamp")]
    [property: Required]
    DateTime Timestamp

);
