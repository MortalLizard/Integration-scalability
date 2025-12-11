using System.Text.Json;

using Json.Schema;

namespace Orchestrator.Gateway.Schemas;

public abstract class AbstractJsonSchema
{
    private readonly JsonSchema _schema;

    protected AbstractJsonSchema()
    {
        _schema = BuildSchema();
    }

    protected abstract JsonSchema BuildSchema();

    public List<string>? Validate(JsonElement jsonElement)
    {
        var result = _schema.Evaluate(
            jsonElement,
            new EvaluationOptions { OutputFormat = OutputFormat.List });

        if (result.IsValid)
            return null;

        var errors = new List<string>();

        foreach (var detail in result.Details)
        {
            if (!detail.IsValid && detail.HasErrors && detail.Errors is not null)
            {
                foreach (var kvp in detail.Errors)
                {
                    errors.Add($"{detail.InstanceLocation}: {kvp.Value}");
                }
            }
        }

        return errors;
    }
}
