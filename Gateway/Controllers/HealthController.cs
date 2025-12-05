using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        //return 200 ok as well as diagnostic data
        [HttpGet]
        public IActionResult GetHealth()
        {
            var healthInfo = new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                application = "Gateway",
                version = "1.0.0"
            };
            return Ok(healthInfo);
        }
    }
}
