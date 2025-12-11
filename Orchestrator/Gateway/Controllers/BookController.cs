using System.Text.Json;
using Gateway.DTOs;
using Microsoft.AspNetCore.Mvc;
using Orchestrator.Gateway.DTOs;
using Orchestrator.Schemas;
using Orchestrator.Utils.Enrichers;
using Orchestrator.Utils.Mappers;

using Serilog;
using Shared;

namespace Orchestrator.Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
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
        public IActionResult NewOrder([FromBody] JsonElement jsonElement, [FromServices] Producer producer)
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

            var orderMessage = dto.ToOrderMessage();

            foreach (var orderLine in dto.Items)
            {
                // Route to the right service
                if (orderLine.Marketplace == true)
                    producer.SendMessageAsync("marketplace.order-item.process", orderLine.ToMarketplaceOrderlineProcess(orderMessage.OrderId)).GetAwaiter().GetResult();
                else
                    producer.SendMessageAsync("inventory.order-item.process", orderLine.ToInventoryOrderlineProcess(orderMessage.OrderId)).GetAwaiter().GetResult();
            }

            // Send it
            producer.SendMessageAsync("new-order.process", orderMessage).GetAwaiter().GetResult();

            return Ok();
        }

    }

}
