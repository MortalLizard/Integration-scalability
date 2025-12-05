using Marketplace.Business.Interfaces;
using Marketplace.Contracts.Commands;

namespace Marketplace.Business.Services;

public class CreateBookLogic : ICreateBookLogic
{
    public Task CreateBook(CreateBook createBook, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
