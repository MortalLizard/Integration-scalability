using Shared.Contracts.CreateBook;

namespace Marketplace.Business.Interfaces;

public interface ICreateBookLogic
{
    public Task CreateBook(MarketplaceBookCreate marketplaceBookCreate, CancellationToken ct = default);
}
