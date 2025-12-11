using System.Text.Json.Serialization;

namespace Shared.Contracts.OrderBook;

public sealed record OrderProcess(

    [property: JsonPropertyName("order_id")]
    [property: JsonRequired]
    Guid OrderId,

    [property: JsonPropertyName("buyer_email")]
    [property: JsonRequired]
    string BuyerEmail,

    [property: JsonPropertyName("total_items")]
    [property: JsonRequired]
    int TotalItems
);
