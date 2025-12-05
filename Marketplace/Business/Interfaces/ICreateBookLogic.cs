
using Marketplace.Contracts.Commands;

namespace Marketplace.Business.Interfaces;

public interface ICreateBookLogic
{
    public Task CreateBook(CreateBook createBook, CancellationToken ct = default);
}
