
using Json.Schema;

namespace Orchestrator.Schemas;

public sealed class MarketplaceBookSchema : AbstractJsonSchema
{
    public static MarketplaceBookSchema Instance { get; } = new();

    private MarketplaceBookSchema()
    {
    }

    protected override JsonSchema BuildSchema()
    {
        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .AdditionalProperties(false)
            .Required("title", "author", "price", "isbn", "description", "published_date", "seller_id")
            .Properties(
                ("title", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                ("author", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                ("price", new JsonSchemaBuilder().Type(SchemaValueType.Number)),
                ("isbn", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                ("published_date", new JsonSchemaBuilder().Type(SchemaValueType.String).Format("date-time")),
                ("seller_id", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                ("description", new JsonSchemaBuilder().Type(SchemaValueType.String))
            )
            .Build();
    }
}
