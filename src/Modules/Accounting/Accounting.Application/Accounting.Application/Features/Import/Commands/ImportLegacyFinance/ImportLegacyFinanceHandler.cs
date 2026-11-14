using Accounting.Application.Abstractions;
using Accounting.Application.Features.Import.Shared;
using Accounting.Application.Features.Import.Shared.Reading;
using Accounting.Application.Features.Import.Shared.Report;

namespace Accounting.Application.Features.Import.Commands.ImportLegacyFinance;

public sealed class ImportLegacyFinanceHandler(
    IAccountingDbContext db,
    IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers)
    : ICommandHandler<ImportLegacyFinanceCommand, Result<LegacyFinanceImportReport>>
{
    private const int PartSize = 2000;

    public async ValueTask<Result<LegacyFinanceImportReport>> Handle(ImportLegacyFinanceCommand command,
        CancellationToken cancellationToken)
    {
        var read = await LegacyFinanceReader.ReadAsync(command.Content, cancellationToken);
        if (read.IsFailure) return read.Error!.ToResult<LegacyFinanceImportReport>();
        var file = read.Value;

        var existing = new LegacyFinanceExisting(
            await db.Accounts.AsNoTracking().ToListAsync(cancellationToken),
            await db.Categories.AsNoTracking().ToListAsync(cancellationToken),
            (await db.Counterparties.AsNoTracking().Select(x => x.Id).ToListAsync(cancellationToken))
                .Select(x => x.Value).ToHashSet(),
            (await db.Transactions.AsNoTracking().Select(x => x.Id).ToListAsync(cancellationToken))
                .Select(x => x.Value).ToHashSet(),
            (await db.Transfers.AsNoTracking().Select(x => x.Id).ToListAsync(cancellationToken))
                .Select(x => x.Value).ToHashSet());
        var clients = await customers.FindByKeepinIdsAsync(LegacyFinancePlan.UsedKeepinIds(file), cancellationToken);
        var supplierList = LegacyFinancePlan.UsesSuppliers(file) ? await suppliers.ListAsync(cancellationToken) : [];

        var plan = LegacyFinancePlan.Build(file, existing, clients, supplierList);
        if (command.DryRun || !plan.Report.CanApply) return Result.Success(plan.Report);

        try
        {
            await using var transaction = await db.BeginTransactionAsync(cancellationToken);
            await SaveAsync(plan.NewAccounts, db.Accounts.AddRange, cancellationToken);
            await SaveAsync(plan.NewCategories, db.Categories.AddRange, cancellationToken);
            await SaveAsync(plan.NewCounterparties, db.Counterparties.AddRange, cancellationToken);
            await SaveAsync(plan.NewTransactions, db.Transactions.AddRange, cancellationToken);
            await SaveAsync(plan.NewTransfers, db.Transfers.AddRange, cancellationToken);
            await SaveAsync(plan.NewAuditEntries, db.AuditEntries.AddRange, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Someone added the same records meanwhile; the transaction is rolled back, nothing is written.
            return Result.Conflict(ImportMessages.Stale);
        }

        return Result.Success(plan.Report with { Applied = true });
    }

    /// <summary>
    /// Saves the records in parts and forgets each saved part, so forty thousand operations do not stay tracked. Roots
    /// of categories come first in their list, so a part never holds a subcategory before its root.
    /// </summary>
    private async Task SaveAsync<T>(IReadOnlyList<T> records, Action<IEnumerable<T>> add,
        CancellationToken cancellationToken)
    {
        foreach (var part in records.Chunk(PartSize))
        {
            add(part);
            await db.SaveChangesAsync(cancellationToken);
            db.ClearTracked();
        }
    }
}
