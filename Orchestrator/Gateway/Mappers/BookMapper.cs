using Gateway.DTOs;

using Shared.Contracts.CreateBook;

namespace Orchestrator.Gateway.Mappers;

public static class BookMapper
{
    extension(MarketplaceBookDto dto)
    {
        public MarketplaceBookCreate ToBookCreate()
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new MarketplaceBookCreate(
                Id: Guid.NewGuid(),
                Title: dto.Title,
                Author: dto.Author,
                Isbn: dto.Isbn,
                Price: dto.Price,
                PublishedDate: dto.PublishedDate,
                Description: dto.Description,
                SellerId: dto.SellerId
            );
        }
    }
}
