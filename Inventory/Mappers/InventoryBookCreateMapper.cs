using Inventory.Database.Entities;

using Shared.Contracts.CreateBook;

namespace Inventory.Mappers;

public static class InventoryBookCreateMapper
{
    extension(Book book)
    {
        public InventoryBookCreated ToBookCreated()
        {
            return new InventoryBookCreated(
                Id: book.Id,
                Title: book.Title,
                Author: book.Author,
                Isbn: book.Isbn,
                Price: book.Price,
                Quantity: book.Quantity,
                PublishedDate: book.PublishedDate,
                Description: book.Description
            );
        }
    }
}
