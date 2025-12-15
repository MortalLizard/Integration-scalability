using Microsoft.EntityFrameworkCore;
using Orchestrator.OrderSaga.Database.DbContext;
using Orchestrator.OrderSaga.Database.Entities;

namespace Orchestrator.OrderSaga.Database.Repository;

public sealed class OrderSagaRepository(OrderDbContext dbContext) : IOrderSagaRepository
{
    public async Task CreateSagaWithLinesAsync(OrderSagaState state, IReadOnlyCollection<OrderSagaLine> lines, CancellationToken ct = default)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

            dbContext.OrderSagas.Add(state);
            dbContext.OrderSagaLines.AddRange(lines);

            await dbContext.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });
    }


    public Task<OrderSagaState?> GetSagaAsync(Guid orderId, CancellationToken ct = default)
        => dbContext.OrderSagas.AsNoTracking().FirstOrDefaultAsync(x => x.OrderId == orderId, ct);

    public async Task<IReadOnlyList<OrderSagaLine>> GetLinesByStatusAsync(Guid orderId, OrderSagaLineStatus status, CancellationToken ct = default)
        => await dbContext.OrderSagaLines
            .AsNoTracking()
            .Where(x => x.OrderId == orderId && x.Status == status)
            .ToListAsync(ct);

    public async Task<bool> TryTransitionSagaStatusAsync(Guid orderId, OrderSagaStatus from, OrderSagaStatus to, CancellationToken ct = default)
    {
        int affected = await dbContext.OrderSagas
            .Where(s => s.OrderId == orderId && s.Status == from)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.Status, to)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow), ct);

        return affected == 1;
    }

    public async Task<bool> TrySetPaymentAuthorizedAsync(Guid orderId, CancellationToken ct = default)
    {
        int affected = await dbContext.OrderSagas
            .Where(s => s.OrderId == orderId
                        && s.Status == OrderSagaStatus.PaymentAndReserve
                        && !s.PaymentAuthorized)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.PaymentAuthorized, true)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow), ct);

        return affected == 1;
    }

    public async Task<bool> TrySetInvoicedAsync(Guid orderId, CancellationToken ct = default)
    {
        int affected = await dbContext.OrderSagas
            .Where(s => s.OrderId == orderId
                        && s.Status == OrderSagaStatus.InvoiceShipSearch
                        && !s.Invoiced)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.Invoiced, true)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow), ct);

        return affected == 1;
    }

    public async Task<bool> TrySetShippedAsync(Guid orderId, CancellationToken ct = default)
    {
        int affected = await dbContext.OrderSagas
            .Where(s => s.OrderId == orderId
                        && s.Status == OrderSagaStatus.InvoiceShipSearch
                        && !s.Shipped)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.Shipped, true)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow), ct);

        return affected == 1;
    }

    public async Task<bool> TrySetSearchUpdatedAsync(Guid orderId, CancellationToken ct = default)
    {
        int affected = await dbContext.OrderSagas
            .Where(s => s.OrderId == orderId
                        && s.Status == OrderSagaStatus.InvoiceShipSearch
                        && !s.SearchUpdated)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.SearchUpdated, true)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow), ct);

        return affected == 1;
    }

    public async Task<bool> TrySetFailureReasonAsync(Guid orderId, string reason, CancellationToken ct = default)
    {
        int affected = await dbContext.OrderSagas
            .Where(s => s.OrderId == orderId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.FailureReason, reason)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow), ct);

        return affected == 1;
    }

    public async Task<bool> TrySetLineReservationSuccessAsync(Guid orderId, Guid lineId, CancellationToken ct = default)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

            int lineAffected = await dbContext.OrderSagaLines
                .Where(l => l.OrderId == orderId && l.LineId == lineId && l.Status == OrderSagaLineStatus.Pending)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(l => l.Status, OrderSagaLineStatus.Reserved)
                    .SetProperty(l => l.UpdatedAt, DateTime.UtcNow), ct);

            if (lineAffected != 1)
            {
                await tx.RollbackAsync(ct);
                return false;
            }

            await dbContext.OrderSagas
                .Where(s => s.OrderId == orderId && s.Status == OrderSagaStatus.PaymentAndReserve)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.LinesCompleted, s => s.LinesCompleted + 1)
                    .SetProperty(s => s.InventoryReserved, s => s.InventoryReserved || (s.LinesCompleted + 1 >= s.LinesExpected))
                    .SetProperty(s => s.UpdatedAt, DateTime.UtcNow), ct);

            await tx.CommitAsync(ct);
            return true;
        });
    }


    public async Task<bool> TrySetLineReservationFailureAsync(Guid orderId, Guid lineId, string reason, CancellationToken ct = default)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await dbContext.Database.BeginTransactionAsync(ct);

            int lineAffected = await dbContext.OrderSagaLines
                .Where(l => l.OrderId == orderId && l.LineId == lineId && l.Status == OrderSagaLineStatus.Pending)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(l => l.Status, OrderSagaLineStatus.Failed)
                    .SetProperty(l => l.FailureReason, reason)
                    .SetProperty(l => l.UpdatedAt, DateTime.UtcNow), ct);

            if (lineAffected != 1)
            {
                await tx.RollbackAsync(ct);
                return false;
            }

            await dbContext.OrderSagas
                .Where(s => s.OrderId == orderId && s.Status == OrderSagaStatus.PaymentAndReserve)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.LinesFailed, s => s.LinesFailed + 1)
                    .SetProperty(s => s.UpdatedAt, DateTime.UtcNow), ct);

            await tx.CommitAsync(ct);
            return true;
        });
    }


    public async Task<bool> TryAdvanceToInvoiceShipSearchAsync(Guid orderId, CancellationToken ct = default)
    {
        int affected = await dbContext.OrderSagas
            .Where(s =>
                s.OrderId == orderId &&
                s.Status == OrderSagaStatus.PaymentAndReserve &&
                s.PaymentAuthorized &&
                s.LinesFailed == 0 &&
                s.LinesCompleted >= s.LinesExpected)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.Status, OrderSagaStatus.InvoiceShipSearch)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow), ct);

        return affected == 1;
    }

    public async Task<bool> TryCompleteAsync(Guid orderId, CancellationToken ct = default)
    {
        int affected = await dbContext.OrderSagas
            .Where(s =>
                s.OrderId == orderId &&
                s.Status == OrderSagaStatus.InvoiceShipSearch &&
                s.Shipped && s.Invoiced && s.SearchUpdated)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.Status, OrderSagaStatus.Completed)
                .SetProperty(s => s.UpdatedAt, DateTime.UtcNow), ct);

        return affected == 1;
    }

    // Late-message helpers (don’t bump counters; you use these only in Compensating/Failed)
    public async Task<bool> TryMarkLineReservedAsync(Guid orderId, Guid lineId, CancellationToken ct = default)
    {
        int affected = await dbContext.OrderSagaLines
            .Where(x => x.OrderId == orderId && x.LineId == lineId && x.Status == OrderSagaLineStatus.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, OrderSagaLineStatus.Reserved)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);

        return affected == 1;
    }

    public async Task<bool> TryMarkLineFailedAsync(Guid orderId, Guid lineId, string reason, CancellationToken ct = default)
    {
        int affected = await dbContext.OrderSagaLines
            .Where(x => x.OrderId == orderId && x.LineId == lineId && x.Status == OrderSagaLineStatus.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, OrderSagaLineStatus.Failed)
                .SetProperty(x => x.FailureReason, reason)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);

        return affected == 1;
    }

    public async Task<bool> TryMarkLineCompensationSentAsync(Guid orderId, Guid lineId, CancellationToken ct = default)
    {
        int affected = await dbContext.OrderSagaLines
            .Where(x => x.OrderId == orderId && x.LineId == lineId && x.Status == OrderSagaLineStatus.Reserved)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, OrderSagaLineStatus.CompensationSent)
                .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);

        return affected == 1;
    }
}
