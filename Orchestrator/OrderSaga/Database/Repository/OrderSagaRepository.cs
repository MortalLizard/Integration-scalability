using Microsoft.EntityFrameworkCore;
using Orchestrator.OrderSaga.Database.DbContext;
using Orchestrator.OrderSaga.Database.Entities;

namespace Orchestrator.OrderSaga.Database.Repository;

public sealed class OrderSagaRepository(OrderDbContext dbContext) : IOrderSagaRepository
{
    public async Task SaveAsync(OrderSagaState state, CancellationToken ct = default)
    {
        var existing = await dbContext.OrderSagas
            .FirstOrDefaultAsync(x => x.OrderId == state.OrderId, ct);

        if (existing is null)
        {
            dbContext.OrderSagas.Add(state);
        }
        else
        {
            dbContext.Entry(existing).CurrentValues.SetValues(state);
        }

        await dbContext.SaveChangesAsync(ct);
    }

    public Task<OrderSagaState?> GetAsync(Guid orderId, CancellationToken ct = default)
        => dbContext.OrderSagas.FirstOrDefaultAsync(x => x.OrderId == orderId, ct);

    public async Task AddLinesAsync(IReadOnlyCollection<OrderSagaLine> lines, CancellationToken ct = default)
    {
        dbContext.OrderSagaLines.AddRange(lines);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<bool> TryAdvanceStatusAsync(Guid orderId, OrderSagaStatus from, OrderSagaStatus to, CancellationToken ct = default)
    {
        var affected = await dbContext.OrderSagas
            .Where(x => x.OrderId == orderId && x.Status == from)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, to)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);

        return affected == 1;
    }

    public async Task<bool> TryMarkLineReservedAsync(Guid orderId, Guid lineId, CancellationToken ct = default)
    {
        var affected = await dbContext.OrderSagaLines
            .Where(x => x.OrderId == orderId && x.LineId == lineId && x.Status == OrderSagaLineStatus.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, OrderSagaLineStatus.Reserved)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);

        return affected == 1;
    }

    public async Task<bool> TryMarkLineFailedAsync(Guid orderId, Guid lineId, string reason, CancellationToken ct = default)
    {
        var affected = await dbContext.OrderSagaLines
            .Where(x => x.OrderId == orderId && x.LineId == lineId && x.Status == OrderSagaLineStatus.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, OrderSagaLineStatus.Failed)
                .SetProperty(x => x.FailureReason, reason)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);

        return affected == 1;
    }

    public Task<int> CountReservedAsync(Guid orderId, CancellationToken ct = default)
        => dbContext.OrderSagaLines.CountAsync(x => x.OrderId == orderId && x.Status == OrderSagaLineStatus.Reserved, ct);

    public Task<int> CountFailedAsync(Guid orderId, CancellationToken ct = default)
        => dbContext.OrderSagaLines.CountAsync(x => x.OrderId == orderId && x.Status == OrderSagaLineStatus.Failed, ct);

    public async Task<IReadOnlyList<OrderSagaLine>> GetLinesByStatusAsync(Guid orderId, OrderSagaLineStatus status, CancellationToken ct = default)
        => await dbContext.OrderSagaLines.Where(x => x.OrderId == orderId && x.Status == status).ToListAsync(ct);

    public async Task<bool> TryMarkLineCompensationSentAsync(Guid orderId, Guid lineId, CancellationToken ct = default)
    {
        var affected = await dbContext.OrderSagaLines
            .Where(x => x.OrderId == orderId && x.LineId == lineId && x.Status == OrderSagaLineStatus.Reserved)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, OrderSagaLineStatus.CompensationSent)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);

        return affected == 1;
    }
}
