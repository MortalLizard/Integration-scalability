using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Gateway.Contracts;
using Gateway.Schemas;

using Json.Schema;
using Microsoft.AspNetCore.Mvc;

using Serilog;

using Shared;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        [HttpPost("marketplace")]
        public IActionResult NewMarketplaceBook([FromBody] JsonElement jsonElement)
        {
            // validation input using schema validation
            var errors = MarketplaceBookSchema.Instance.Validate(jsonElement);
            if (errors is not null)
            {
                Log.Error("Marketplace book payload failed schema validation: {Errors}", JsonSerializer.Serialize(errors));
                return BadRequest(new { message = "Invalid payload", errors });
            }

            //create the dto
            MarketplaceBookDto? dto;
            try
            {
                dto = JsonSerializer.Deserialize<MarketplaceBookDto>(
                    jsonElement.GetRawText(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (JsonException ex)
            {
                Log.Error(ex, "Deserialization failed for marketplace book payload: {Payload}", jsonElement.GetRawText());
                return BadRequest(new { message = "Deserialization failed", error = ex.Message });
            }

            if (dto is null)
            {
                return BadRequest(new { message = "Payload deserialized to null" });
            }

            var outboundJson = JsonSerializer.Serialize(dto);
            //send it
            var producer = new Producer();
            producer.SendMessageAsync("marketplace_books", outboundJson).GetAwaiter().GetResult();

            return Ok();
        }

        [HttpPost("neworder")]
        public IActionResult NewOrder([FromBody] JsonElement jsonElement)
        {
            // validation input using schema validation
            var errors = OrderSchema.Instance.Validate(jsonElement);
            if (errors is not null)
            {
                Log.Warning("Order payload failed schema validation: {Errors}", JsonSerializer.Serialize(errors));
                return BadRequest(new { message = "Invalid payload", errors });
            }

            OrderDto? dto;
            try
            {
                dto = JsonSerializer.Deserialize<OrderDto>(
                    jsonElement.GetRawText(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                if (dto is null)
                {
                    Log.Warning("Order payload deserialized to null");
                    return BadRequest(new { message = "Payload deserialized to null" });
                }

                dto.OrderId = Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error during order deserialization");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
            var producer = new Producer();

            foreach (var orderLine in dto.Items)
            {
                //serialize the orderline
                orderLine.CorrelationId = dto.OrderId;
                var ol = JsonSerializer.Serialize(orderLine);
                //Send to the right topic
                if (orderLine.Marketplace == true)
                    producer.SendMessageAsync("marketplace.order-item.process", ol).GetAwaiter().GetResult();
                else
                    producer.SendMessageAsync("inventory.order-item.process", ol).GetAwaiter().GetResult();
            }
            //unset the items to reduce message size
            dto.Items.Clear();
            //serialize the order without items
            var outboundJson = JsonSerializer.Serialize(dto);
            //send it
            producer.SendMessageAsync("new-order.process", outboundJson).GetAwaiter().GetResult();

            return Ok(outboundJson);
        }

    }
}
