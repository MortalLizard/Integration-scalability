using Inventory.Database.Entities;

namespace Inventory.Database.Services;

public interface IBookService
{
    Task<List<Book>> GetAllAsync();
    Task<Book?> GetByIdAsync(Guid id);
    Task CreateAsync(Book book);
    Task<bool> UpdateAsync(Book book);
    Task<bool> DeleteAsync(Guid id);


    Task<Book?> UpdateStockAsync(Guid id, int quantityChange);
}
