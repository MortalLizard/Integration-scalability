using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marketplace.DTOs;

public sealed record CreateBook(

    [property: JsonPropertyName("id")]
    [property: Required]
    Guid Id,

    [property: JsonPropertyName("title")]
    [property: Required]
    string Title,

    [property: JsonPropertyName("author")]
    string? Author,

    [property: JsonPropertyName("isbn")]
    string? Isbn,

    [property: JsonPropertyName("price")]
    [property: Range(typeof(decimal), "0", "999999999")]
    [property: Required]
    decimal Price,

    [property: JsonPropertyName("publishedDate")]
    DateTime? PublishedDate,

    [property: JsonPropertyName("description")]
    string? Description
);
