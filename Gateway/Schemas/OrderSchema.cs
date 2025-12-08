using Json.Schema;

namespace Gateway.Schemas;

public sealed class OrderSchema : AbstractJsonSchema
{
    public static OrderSchema Instance { get; } = new();

    private OrderSchema()
    {

    }

    protected override JsonSchema BuildSchema()
    {
        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .AdditionalProperties(false)
            .Required("buyer_email", "items")
            .Properties(
                ("buyer_email", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                ("items", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Array)
                    .Items(
                        new JsonSchemaBuilder()
                            .Type(SchemaValueType.Object)
                            .AdditionalProperties(false)
                            .Required("book_id", "quantity", "marketplace", "price")
                            .Properties(
                                ("book_id", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                                ("quantity", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
                                ("marketplace", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
                                ("price", new JsonSchemaBuilder().Type(SchemaValueType.Number))
                    ))
                )
            )
            .Build();
    }
}
