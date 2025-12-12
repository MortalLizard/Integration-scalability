using Shared.Contracts.CreateBook;

namespace Shared.Contracts.Mappers;

public static class MarketplaceBookCreateMapper
{
    extension(MarketplaceBookCreate create)
    {
        public MarketplaceBookCreated ToBookCreated()
        {
            return new MarketplaceBookCreated(
                Id: create.Id,
                Title: create.Title,
                Author: create.Author,
                Isbn: create.Isbn,
                Price: create.Price,
                PublishedDate: create.PublishedDate,
                Description: create.Description,
                SellerId: create.SellerId
            );
        }
    }
}
