using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;

using Search.Models;

namespace Search.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookController : ControllerBase
{
    private readonly ElasticsearchClient _elastic;

    public BookController(ElasticsearchClient elastic)
    {
        _elastic = elastic;
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks()
    {
        // basic search on `books` index
        var response = await _elastic.SearchAsync<Book>(s => s
            .Indices("books")
            .Query(q => q.MatchAll())
        );

        if (!response.IsValidResponse)
            return StatusCode(500, response.ElasticsearchServerError?.Error.Reason);

        return Ok(response.Documents);
    }

    // POST api/book/seed
    [HttpPost("seed")]
    public async Task<IActionResult> SeedBooks()
    {
        var now = DateTime.UtcNow;

        var book = new Book
        {
            Title = "Full-Text Search with .NET",
            Author = "Bob",
            Isbn = "9780000000002",
            Price = 29.99m,
            PublishedDate = now.AddMonths(-6),
            Description = "Full-text search patterns in C#.",
            Origin = "internal",
            SellerId = "seller-2"
        };

        var response = await _elastic.IndexAsync(book, x => x.Index("books"));

        if (!response.IsValidResponse)
            return StatusCode(500, response.ElasticsearchServerError?.Error.Reason);

        return Ok(new { indexed = response.Index });
    }
}
