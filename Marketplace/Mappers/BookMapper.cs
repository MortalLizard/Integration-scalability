using Marketplace.Database.Entities;
using Shared.Contracts.CreateBook;

namespace Marketplace.Mappers;

public static class CreateBookMapper
{
    extension(MarketplaceBookCreate createMarketplaceBook)
    {
        public Book ToEntity()
        {
            var now = DateTime.UtcNow;

            return new Book
            {
                Id = createMarketplaceBook.Id,
                Title = createMarketplaceBook.Title,
                Author = createMarketplaceBook.Author,
                Isbn = createMarketplaceBook.Isbn,
                Price = createMarketplaceBook.Price,
                PublishedDate = createMarketplaceBook.PublishedDate,
                Description = createMarketplaceBook.Description,
                IsActive = true,
                SellerId = createMarketplaceBook.SellerId,
                CreatedAt = now,
                UpdatedAt = now
            };
        }
    }
}
