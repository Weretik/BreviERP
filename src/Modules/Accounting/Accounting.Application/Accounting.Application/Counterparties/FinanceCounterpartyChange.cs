using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Contracts;
using Accounting.Application.Lookup;
using Accounting.Domain.Audit;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;

namespace Accounting.Application.Counterparties;

/// <summary>
/// Shared steps of a finance counterparty change: find it, check the version the user saw, apply the change, write the
/// log entry and save. A change that alters nothing keeps the version and writes no entry.
/// </summary>
internal static class FinanceCounterpartyChange
{
    /// <param name="change">Applies the change and returns its log action, or null when nothing changed.</param>
    public static async Task<Result<FinanceCounterpartyResponse>> ApplyAsync(IAccountingDbContext db, Guid id, int version,
        string by, DateTimeOffset now, Func<FinanceCounterparty, Task<AuditAction?>> change,
        CancellationToken cancellationToken)
    {
        var counterparty = await FindAsync(db, id, cancellationToken);
        if (counterparty is null) return Result.NotFound(AccountingMessages.CounterpartyNotFound);
        if (counterparty.Version != version) return Result.Conflict(AccountingMessages.Stale);

        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        var before = AuditTrail.Snapshot(counterparty, names);
        var action = await change(counterparty);
        if (action is not null)
        {
            AuditTrail.Add(db, AuditEntity.Counterparty, id, action.Value, by, now, before,
                AuditTrail.Snapshot(counterparty, names));
            var saved = await SaveAsync(db, cancellationToken);
            if (!saved.IsSuccess) return Result.Conflict(AccountingMessages.Stale);
        }

        return Result.Success(await ResponseAsync(db, counterparty, names, cancellationToken));
    }

    public static Task<FinanceCounterparty?> FindAsync(IAccountingDbContext db, Guid id, CancellationToken cancellationToken)
    {
        var key = FinanceCounterpartyId.From(id);
        return db.Counterparties.FirstOrDefaultAsync(x => x.Id == key, cancellationToken);
    }

    public static async Task<Result> SaveAsync(IAccountingDbContext db, CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(AccountingMessages.Stale);
        }
    }

    public static async Task<FinanceCounterpartyResponse> ResponseAsync(IAccountingDbContext db,
        FinanceCounterparty counterparty, FinanceNames names, CancellationToken cancellationToken)
    {
        var counts = await OperationCountsAsync(db, CounterpartyKind.Financial, [counterparty.Link.Id], cancellationToken);
        return FinanceCounterpartyResponse.From(counterparty, names, counts.GetValueOrDefault(counterparty.Link.Id));
    }

    /// <summary>Active operations of each counterparty of one kind, both areas.</summary>
    public static async Task<Dictionary<string, int>> OperationCountsAsync(IAccountingDbContext db, CounterpartyKind kind,
        IReadOnlyCollection<string> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0) return [];
        var rows = await db.Transactions.AsNoTracking()
            .Where(x => x.Status == MovementStatus.Active && x.CounterpartyKind == kind && ids.Contains(x.CounterpartyId!))
            .GroupBy(x => x.CounterpartyId!)
            .Select(g => new { Id = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(x => x.Id, x => x.Count);
    }

    /// <summary>Operations linked to a finance counterparty, cancelled ones too, tracked for a change.</summary>
    public static Task<List<FinanceTransaction>> OperationsOfAsync(IAccountingDbContext db, FinanceCounterparty counterparty,
        CancellationToken cancellationToken)
    {
        var id = counterparty.Link.Id;
        return db.Transactions.Where(x => x.CounterpartyKind == CounterpartyKind.Financial && x.CounterpartyId == id)
            .ToListAsync(cancellationToken);
    }
}
