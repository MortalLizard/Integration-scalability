using Inventory.Database.Entities;

namespace Inventory.Database.Services;

public interface IBookService
{
    public Task<bool> UpdateAsync(Book book);
}
