using Microsoft.EntityFrameworkCore;

using Orchestrator.OrderSaga.Database.DbContext;
using Orchestrator.OrderSaga.Database.Entities;

namespace Orchestrator.OrderSaga.Database.Repository;

public sealed class OrderSagaRepository(OrderDbContext dbContext) : IOrderSagaRepository
{
    public async Task<OrderSagaState?> GetAsync(
        Guid orderId,
        CancellationToken ct = default)
    {
        return await dbContext.OrderSagas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
    }

    public async Task SaveAsync(
        OrderSagaState state,
        CancellationToken ct = default)
    {
        var existing = await dbContext.OrderSagas
            .FirstOrDefaultAsync(x => x.OrderId == state.OrderId, ct);

        if (existing is null)
        {
            dbContext.OrderSagas.Add(state);
        }
        else
        {
            // copy values onto tracked entity
            dbContext.Entry(existing).CurrentValues.SetValues(state);
        }

        await dbContext.SaveChangesAsync(ct);
    }
}
