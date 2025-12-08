using System.Text.Json.Serialization;

namespace Marketplace.Contracts.Commands;

public sealed record OrderItemProcess(

    [property: JsonPropertyName("correlation_id")]
    [property: JsonRequired]
    Guid CorrelationId,

    [property: JsonPropertyName("book_id")]
    [property: JsonRequired]
    Guid BookId
);
