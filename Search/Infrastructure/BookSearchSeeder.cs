using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Search.Models;

namespace Search.Infrastructure;

public class BookSearchSeeder
{
    private readonly ElasticsearchClient _elastic;

    public BookSearchSeeder(ElasticsearchClient elastic)
    {
        _elastic = elastic;
    }

    public async Task SeedAsync(int targetCount = 3000, CancellationToken cancellationToken = default)
    {
        const string indexName = "books";

        // Check if index exists
        var existsResponse = await _elastic.Indices.ExistsAsync(indexName, cancellationToken: cancellationToken);
        if (!existsResponse.IsValidResponse)
        {
            throw new Exception($"Failed to check if index '{indexName}' exists: {existsResponse.ElasticsearchServerError?.Error.Reason}");
        }


        // Check current document count to keep operation idempotent-ish
        var countResponse = await _elastic.CountAsync<Book>(c => c.Indices(indexName), cancellationToken);
        if (!countResponse.IsValidResponse)
        {
            throw new Exception($"Failed to count documents in index '{indexName}': {countResponse.ElasticsearchServerError?.Error.Reason}");
        }

        if (countResponse.Count >= targetCount)
        {
            // Already seeded enough docs
            return;
        }

        var remaining = targetCount - (int)countResponse.Count;
        var batchSize = 500;
        var random = new Random(42);

        var booksToIndex = GenerateBooks(remaining, random);
        var bulkAll = _elastic.BulkAll(booksToIndex, b => b
            .Index(indexName)
            .BackOffRetries(2)
            .BackOffTime("30s")
            .RefreshOnCompleted()
            .Size(batchSize), cancellationToken);

        bulkAll.Wait(TimeSpan.FromMinutes(10), _ => { });
    }

    private static IEnumerable<Book> GenerateBooks(int count, Random random)
    {
        var now = DateTime.UtcNow;
        var titles = new[]
        {
            "Full-Text Search with .NET",
            "Introduction to Elasticsearch",
            "C# in Depth",
            "Clean Code in C#",
            "Distributed Systems with .NET",
            "Microservices with ASP.NET Core",
            "Modern Web APIs",
            "Patterns of Enterprise Application Architecture",
            "Domain-Driven Design with C#",
            "Event-Driven Architecture"
        };

        var authors = new[] { "Alice", "Bob", "Charlie", "Diana", "Eve", "Frank" };

        for (var i = 0; i < count; i++)
        {
            var title = titles[random.Next(titles.Length)] + $" Vol.{random.Next(1, 10)}";
            var author = authors[random.Next(authors.Length)];
            var isbn = $"978-{random.Next(100000000, 999999999)}";
            var price = Math.Round((decimal)(random.NextDouble() * 90.0 + 10.0), 2); // 10 - 100
            var publishedDate = now.AddDays(-random.Next(0, 3650)); // last 10 years

            yield return new Book
            {
                Title = title,
                Author = author,
                Isbn = isbn,
                Price = price,
                PublishedDate = publishedDate,
                Description = $"Sample description about {title} by {author}. Covers search, indexing, and .NET patterns.",
                Origin = "seed",
                SellerId = $"seller-{random.Next(1, 50)}"
            };
        }
    }
}

