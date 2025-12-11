using Marketplace.Database.Repositories;

using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookController(IBookRepository bookRepository) : ControllerBase
{
    // GET: api/<BookController>
    [HttpGet("{id:guid}")]
    public IActionResult GetBook(Guid id)
    {
        var book = bookRepository.GetById(id);

        if (book == null) return NotFound();

        return Ok(book);
    }
}
