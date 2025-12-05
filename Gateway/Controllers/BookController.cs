using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Gateway.Contracts;
using Gateway.Schemas;

using Json.Schema;
using Microsoft.AspNetCore.Mvc;

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
                return BadRequest(new { message = "Deserialization failed", error = ex.Message });
            }

            if (dto is null)
                return BadRequest(new { message = "Payload deserialized to null" });

            var outboundJson = JsonSerializer.Serialize(dto);
            //send it
            var producer = new Producer();
            producer.SendMessageAsync("marketplace_books", outboundJson).GetAwaiter().GetResult();

            return Ok();
        }

    }
}
