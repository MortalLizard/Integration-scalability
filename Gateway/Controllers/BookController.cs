using System.Text.Json;
using Gateway.DTOs;
using Gateway.Schemas;
using Gateway.Utils.Mappers;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using MassTransit;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController(ISendEndpointProvider sendEndpointProvider) : ControllerBase
    {
        [HttpPost("marketplace")]
        public async Task<IActionResult> NewMarketplaceBook([FromBody] JsonElement jsonElement)
        {
            // validation input using schema validation
            var errors = MarketplaceBookSchema.Instance.Validate(jsonElement);
            if (errors is not null)
            {
                Log.Error("Marketplace book payload failed schema validation: {Errors}",
                    JsonSerializer.Serialize(errors));
                return BadRequest(new { message = "Invalid payload", errors });
            }

            var dto = JsonSerializer.Deserialize<MarketplaceBookDto>(
                jsonElement.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto is null) return BadRequest(new { message = "Payload deserialized to null" });

            await sendEndpointProvider.Send(dto.ToBookCreate());

            return Ok();
        }

        [HttpPost("order")]
        public async Task<IActionResult> Order([FromBody] JsonElement jsonElement)
        {
            var errors = OrderSchema.Instance.Validate(jsonElement);
            if (errors is not null)
            {
                Log.Warning("Order payload failed schema validation: {Errors}",
                    JsonSerializer.Serialize(errors));
                return BadRequest(new { message = "Invalid payload", errors });
            }

            var dto = JsonSerializer.Deserialize<OrderDto>(
                jsonElement.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto is null)
            {
                Log.Warning("Order payload deserialized to null");
                return BadRequest(new { message = "Payload deserialized to null" });
            }

            var orderMessage = dto.ToOrderMessage();

            foreach (var orderLine in dto.Items)
            {
                if (orderLine.Marketplace)
                {
                    var marketplaceOrderLineProcess = orderLine.ToMarketplaceOrderlineProcess(orderMessage.OrderId);
                    await sendEndpointProvider.Send(marketplaceOrderLineProcess);
                }
                else
                {
                    var inventoryOrderlineProces = orderLine.ToInventoryOrderlineProcess(orderMessage.OrderId);
                    await sendEndpointProvider.Send(inventoryOrderlineProces);
                }
            }

            return Ok();
        }
    }
}
