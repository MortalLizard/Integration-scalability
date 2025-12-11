using Shared.Contracts.CreateBook;

namespace Marketplace.Business.Interfaces;

public interface ICreateBookLogic
{
    public Task CreateBook(BookCreate bookCreate, CancellationToken ct = default);
}
