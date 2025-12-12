using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookController : ControllerBase
{
    // GET: api/<BookController>
    [HttpGet("{id}")] // should be replaced with an actual id.
    public IActionResult GetBook(int id)
    {
        return NotFound();
    }
}
