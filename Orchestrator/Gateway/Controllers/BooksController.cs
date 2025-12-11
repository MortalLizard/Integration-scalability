using System.Text.Json;
using Gateway.DTOs;
using Microsoft.AspNetCore.Mvc;
using Orchestrator.Gateway.DTOs;
using Orchestrator.Gateway.Mappers;
using Orchestrator.Schemas;

using Serilog;
using Shared;

namespace Orchestrator.Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        [HttpPost("marketplace")]
        public IActionResult NewMarketplaceBook([FromBody] JsonElement jsonElement, [FromServices] Producer producer)
        {
            // Validation input using schema validation
            var errors = MarketplaceBookSchema.Instance.Validate(jsonElement);
            if (errors is not null)
            {
                Log.Warning("Marketplace book payload failed schema validation: {Errors}", JsonSerializer.Serialize(errors));
                return BadRequest(new { message = "Invalid payload", errors });
            }

            // Deserialize the payload
            var dto = JsonSerializer.Deserialize<MarketplaceBookDto>(jsonElement.GetRawText());

            if (dto is null)
            {
                Log.Warning("Order payload deserialized to null");
                return BadRequest(new { message = "Payload deserialized to null" });
            }

            // Send it
            producer.SendMessageAsync("marketplace_books",dto.ToBookCreate()).GetAwaiter().GetResult();

            return Ok();
        }

        [HttpPost("neworder")]
        public IActionResult NewOrder([FromBody] JsonElement jsonElement, [FromServices] IOrderProcessManager pm, CancellationToken ct)
        {
            // validation input using schema validation
            var errors = OrderSchema.Instance.Validate(jsonElement);
            if (errors is not null)
            {
                Log.Warning("Order payload failed schema validation: {Errors}", JsonSerializer.Serialize(errors));
                return BadRequest(new { message = "Invalid payload", errors });
            }

            var dto = JsonSerializer.Deserialize<OrderDto>(jsonElement.GetRawText());

            if (dto is null)
            {
                Log.Warning("Order payload deserialized to null");
                return BadRequest(new { message = "Payload deserialized to null" });
            }

            pm.HandleNewOrderAsync(dto, ct);

            return Ok();
        }

    }

}
