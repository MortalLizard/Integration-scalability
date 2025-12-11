using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

using Microsoft.AspNetCore.Mvc;

using Search.Models;

namespace Search.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
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

    [HttpGet("search")]
    public async Task<IActionResult> SearchBooks(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var from = (page - 1) * pageSize;

        var response = await _elastic.SearchAsync<Book>(s => s
            .Indices("books")
            .From(from)
            .Size(pageSize)
            .Query(q => q
                .MultiMatch(m => m
                    .Fields(new[] { "title", "description" })
                    .Query(query)
                )
            )
        );

        if (!response.IsValidResponse)
            return StatusCode(500, response.ElasticsearchServerError?.Error.Reason);

        return Ok(new
        {
            total = response.HitsMetadata?.Total ?? 0,
            page,
            pageSize,
            items = response.Documents
        });
    }

}


