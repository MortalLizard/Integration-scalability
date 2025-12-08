using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Marketplace.Contracts.Commands;

public sealed record OrderItemProcess(

    [property: JsonPropertyName("correlation_id")]
    [property: Required]
    Guid CorrelationId,

    [property: JsonPropertyName("book_id")]
    [property: Required]
    Guid BookId
);
