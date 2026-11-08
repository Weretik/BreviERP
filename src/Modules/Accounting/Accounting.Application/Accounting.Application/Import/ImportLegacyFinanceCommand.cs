using Accounting.Application.Abstractions;
using Accounting.Application.Contracts;

namespace Accounting.Application.Import;

/// <summary>
/// «Імпорт архіву Фінансів»: checks the export of the old Calculator (<c>DryRun</c>) or moves it into Brevi in one
/// transaction. A move with errors writes nothing and returns the report.
/// </summary>
public sealed record ImportLegacyFinanceCommand(Stream Content, bool DryRun)
    : ICommand<Result<LegacyFinanceImportReport>>;

public sealed class ImportLegacyFinanceCommandValidator : AbstractValidator<ImportLegacyFinanceCommand>
{
    public ImportLegacyFinanceCommandValidator() => RuleFor(x => x.Content).NotNull();
}

public sealed class ImportLegacyFinanceCommandHandler(
    IAccountingDbContext db,
    IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers)
    : ICommandHandler<ImportLegacyFinanceCommand, Result<LegacyFinanceImportReport>>
{
    private const int PartSize = 2000;

    public async ValueTask<Result<LegacyFinanceImportReport>> Handle(ImportLegacyFinanceCommand command,
        CancellationToken cancellationToken)
    {
        LegacyFinanceFile? file;
        try
        {
            file = await LegacyFinanceReader.ReadAsync(command.Content, cancellationToken);
        }
        catch (LegacyFinanceFileException error)
        {
            return Invalid(error.Message);
        }

        if (file is null) return Invalid(ImportMessages.NotJson);
        if (file.Format is null && file.Transactions is null) return Invalid(ImportMessages.NotJson);

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

    private static Result<LegacyFinanceImportReport> Invalid(string message) =>
        Result.Invalid(new ValidationError("File", message));
}
