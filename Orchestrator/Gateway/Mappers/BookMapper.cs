using Gateway.DTOs;

using Shared.Contracts.CreateBook;

namespace Orchestrator.Gateway.Mappers;

public static class BookMapper
{
    extension(MarketplaceBookDto dto)
    {
        public BookCreate ToBookCreate()
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new BookCreate(
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
