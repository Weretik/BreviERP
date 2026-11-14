using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Contracts;
using Accounting.Application.Counterparties;
using Accounting.Application.Journal;
using Accounting.Application.Lookup;
using Accounting.Domain.Audit;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Movements;
using Accounting.Domain.Statements;
using Accounting.Domain.Transactions;
using Accounting.Domain.Transfers;

namespace Accounting.Application.Statements;

/// <summary>
/// «Відкотити виписку»: cancels every active operation and transfer the statement booked (they stay in the journal as
/// cancelled and give up their bank identity, so the lines are no duplicates when booked again), archives the unconfirmed finance counterparties the booking created when nothing active uses them, and
/// turns the statement into a preview that can be corrected and booked again. One save; a stale
/// <paramref name="Version"/> gets 409.
/// </summary>
public sealed record RollbackBankStatementCommand(Guid Id, int Version, string By) : ICommand<Result<BankStatementResponse>>;

public sealed class RollbackBankStatementCommandValidator : AbstractValidator<RollbackBankStatementCommand>
{
    public RollbackBankStatementCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class RollbackBankStatementCommandHandler(IAccountingDbContext db, IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers, TimeProvider clock)
    : ICommandHandler<RollbackBankStatementCommand, Result<BankStatementResponse>>
{
    public async ValueTask<Result<BankStatementResponse>> Handle(RollbackBankStatementCommand command,
        CancellationToken cancellationToken)
    {
        var id = BankStatementId.From(command.Id);
        var statement = await db.BankStatements.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (statement is null) return Result.NotFound(StatementMessages.NotFound);
        if (statement.Version != command.Version) return Result.Conflict(AccountingMessages.Stale);
        var rollback = statement.Rollback();
        if (rollback.IsFailure) return rollback.Error!.ToResult<BankStatementResponse>();

        var lines = await db.BankStatementLines
            .Where(x => x.StatementId == id && (x.TransactionId != null || x.TransferId != null))
            .ToListAsync(cancellationToken);
        var transactionIds = lines.Select(x => x.TransactionId).OfType<FinanceTransactionId>().ToList();
        var transferIds = lines.Select(x => x.TransferId).OfType<FinanceTransferId>().ToList();
        var transactions = await db.Transactions.Where(x => transactionIds.Contains(x.Id)).ToListAsync(cancellationToken);
        var transfers = await db.Transfers.Where(x => transferIds.Contains(x.Id)).ToListAsync(cancellationToken);

        var names = (await FinanceNames.LoadAsync(db, cancellationToken)).With(await CounterpartyStates.LoadAsync(
            new CounterpartyReaders(db, customers, suppliers), transactions.Select(x => x.Counterparty),
            cancellationToken));
        var now = clock.GetUtcNow();
        // A record cancelled by hand before stays a known bank line; the ones cancelled here give up their identity.
        foreach (var transaction in transactions.Where(x => x.Status == MovementStatus.Active))
        {
            var before = AuditTrail.Snapshot(transaction, names);
            // Only active records are taken, so cancelling cannot fail.
            transaction.Void(now).ThrowIfFailure();
            AuditTrail.Add(db, AuditEntity.Transaction, transaction.Id.Value, AuditAction.Void, command.By, now, before,
                AuditTrail.Snapshot(transaction, names));
            transaction.ReleaseBankIdentity();
        }

        foreach (var transfer in transfers.Where(x => x.Status == MovementStatus.Active))
        {
            var before = AuditTrail.Snapshot(transfer, names);
            transfer.Void(now).ThrowIfFailure();
            AuditTrail.Add(db, AuditEntity.Transfer, transfer.Id.Value, AuditAction.Void, command.By, now, before,
                AuditTrail.Snapshot(transfer, names));
            transfer.ReleaseBankIdentity();
        }

        var kept = await ArchiveCreatedAsync(lines, transactionIds, names, command.By, now, cancellationToken);
        foreach (var line in lines)
            line.Unbook(line.Counterparty is { } link ? kept.GetValueOrDefault(link) : null);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(AccountingMessages.Stale);
        }

        return Result.Success(BankStatementResponse.From(statement, names.Account(statement.AccountId)));
    }

    /// <summary>
    /// Archives each finance counterparty the booking created that is still unconfirmed and has no active operation
    /// outside this statement; returns the states of the ones that stay.
    /// </summary>
    private async Task<Dictionary<CounterpartyLink, CounterpartyState>> ArchiveCreatedAsync(
        IEnumerable<BankStatementLine> lines, IReadOnlyCollection<FinanceTransactionId> booked, FinanceNames names,
        string by, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var ids = lines.Where(x => x.CreatedCounterparty && x.Counterparty is { Kind: CounterpartyKind.Financial })
            .Select(x => Guid.TryParse(x.Counterparty!.Id, out var guid) ? FinanceCounterpartyId.From(guid) : (FinanceCounterpartyId?)null)
            .OfType<FinanceCounterpartyId>().Distinct().ToList();
        if (ids.Count == 0) return [];

        var records = await db.Counterparties.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
        var linkIds = records.Select(x => x.Link.Id).ToList();
        var used = await db.Transactions
            .Where(x => x.CounterpartyKind == CounterpartyKind.Financial && linkIds.Contains(x.CounterpartyId!)
                && x.Status == MovementStatus.Active && !booked.Contains(x.Id))
            .Select(x => x.CounterpartyId!).Distinct().ToListAsync(cancellationToken);

        var kept = new Dictionary<CounterpartyLink, CounterpartyState>();
        foreach (var record in records)
        {
            if (record.Verified || !record.Active || record.MergedIntoId is not null || used.Contains(record.Link.Id))
            {
                kept[record.Link] = record.State;
                continue;
            }

            var before = AuditTrail.Snapshot(record, names);
            // A merged record was kept above, so archiving cannot fail.
            record.SetActive(false, now).WithoutValue().ThrowIfFailure();
            AuditTrail.Add(db, AuditEntity.Counterparty, record.Id.Value, AuditAction.Archive, by, now, before,
                AuditTrail.Snapshot(record, names));
        }

        return kept;
    }
}
