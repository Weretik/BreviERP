using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Contracts;
using Accounting.Application.Counterparties;
using Accounting.Application.Journal;
using Accounting.Application.Lookup;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Statements;
using Accounting.Domain.Transactions;
using Accounting.Domain.Transfers;

namespace Accounting.Application.Statements;

/// <summary>
/// «Провести виписку»: every included line that can be booked becomes an operation or a transfer in one save, after
/// the duplicates are checked again. A line with an unknown counterparty creates an unconfirmed finance counterparty
/// (one per ЄДРПОУ, IBAN or name in the statement). A stale <paramref name="Version"/> gets 409.
/// </summary>
public sealed record BookBankStatementCommand(Guid Id, int Version, string By) : ICommand<Result<BankStatementResponse>>;

public sealed class BookBankStatementCommandValidator : AbstractValidator<BookBankStatementCommand>
{
    public BookBankStatementCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Version).GreaterThan(0);
        RuleFor(x => x.By).ValidBy();
    }
}

public sealed class BookBankStatementCommandHandler(IAccountingDbContext db, IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers, TimeProvider clock)
    : ICommandHandler<BookBankStatementCommand, Result<BankStatementResponse>>
{
    public async ValueTask<Result<BankStatementResponse>> Handle(BookBankStatementCommand command,
        CancellationToken cancellationToken)
    {
        var id = BankStatementId.From(command.Id);
        var statement = await db.BankStatements.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (statement is null) return Result.NotFound(StatementMessages.NotFound);
        if (statement.Version != command.Version) return Result.Conflict(AccountingMessages.Stale);
        statement.EnsurePreview();

        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        var account = await db.Accounts.AsNoTracking().FirstAsync(x => x.Id == statement.AccountId, cancellationToken);
        var lines = await db.BankStatementLines
            .Where(x => x.StatementId == id && !x.Excluded && x.State != BankLineState.Error
                && x.Duplicate != BankDuplicate.Exact)
            .OrderBy(x => x.RowNumber).ToListAsync(cancellationToken);

        // The books may have got some of these lines since the upload (another statement, a hand-made entry).
        if (lines.Count > 0)
        {
            var duplicates = await BankDuplicates.LoadAsync(db, account, lines, cancellationToken);
            foreach (var line in lines.ToList())
                if (duplicates.Find(line) is { Duplicate: BankDuplicate.Exact } found)
                {
                    line.FoundInBooks(found.Note);
                    lines.Remove(line);
                }
        }

        var now = clock.GetUtcNow();
        var readers = new CounterpartyReaders(db, customers, suppliers);
        var states = await CounterpartyStates.LoadAsync(readers, lines.Select(x => x.Counterparty), cancellationToken);
        var created = CreateCounterparties(lines, statement, now);
        names.With(new CounterpartyStates(lines.Select(x => x.Counterparty).OfType<CounterpartyLink>().Distinct()
            .Select(x => states.Of(x) ?? created.GetValueOrDefault(x)).OfType<CounterpartyState>()
            .ToDictionary(x => x.Link)));
        foreach (var counterparty in created.Values)
        {
            var record = db.Counterparties.Local.First(x => x.Link == counterparty.Link);
            AuditTrail.Add(db, AuditEntity.Counterparty, record.Id.Value, AuditAction.Create, command.By, now, null,
                AuditTrail.Snapshot(record, names));
        }

        var accounts = await db.Accounts.AsNoTracking().ToDictionaryAsync(x => x.Id, cancellationToken);
        var categoryIds = lines.Select(x => x.CategoryId).OfType<FinanceCategoryId>().Distinct().ToList();
        var categories = await db.Categories.AsNoTracking().Where(x => categoryIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        int transactions = 0, transfers = 0;
        foreach (var line in lines)
        {
            if (line.TransferAccountId is { } otherId)
            {
                if (!accounts.TryGetValue(otherId, out var other)) return Result.NotFound(StatementMessages.TransferAccountNotFound);
                var transfer = FinanceTransfer.FromBank(FinanceTransferId.New(), line, account, other, command.By, now);
                db.Transfers.Add(transfer);
                line.Booked(transfer.Id);
                AuditTrail.Add(db, AuditEntity.Transfer, transfer.Id.Value, AuditAction.Create, command.By, now, null,
                    AuditTrail.Snapshot(transfer, names));
                transfers++;
                continue;
            }

            var category = line.CategoryId is { } categoryId ? categories.GetValueOrDefault(categoryId) : null;
            if (line.CategoryId is not null && category is null) return Result.NotFound(AccountingMessages.CategoryNotFound);
            var state = line.Counterparty is { } link ? names.Counterparties.Of(link) : null;
            if (line.Counterparty is not null && state is null) return Result.NotFound(AccountingMessages.CounterpartyNotFound);
            var transaction = FinanceTransaction.FromBank(FinanceTransactionId.New(), line, account, category, state,
                command.By, now);
            db.Transactions.Add(transaction);
            line.Booked(transaction.Id);
            AuditTrail.Add(db, AuditEntity.Transaction, transaction.Id.Value, AuditAction.Create, command.By, now, null,
                AuditTrail.Snapshot(transaction, names));
            transactions++;
        }

        statement.Book(transactions, transfers, command.By, now);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(AccountingMessages.Stale);
        }

        return Result.Success(BankStatementResponse.From(statement, account.Name));
    }

    /// <summary>
    /// One unconfirmed finance counterparty per ЄДРПОУ, else IBAN, else bank name for the lines nobody was recognised
    /// for; the lines are linked to it.
    /// </summary>
    private Dictionary<CounterpartyLink, CounterpartyState> CreateCounterparties(IEnumerable<BankStatementLine> lines,
        BankStatement statement, DateTimeOffset now)
    {
        var created = new Dictionary<string, FinanceCounterparty>();
        foreach (var line in lines.Where(x => x.TransferAccountId is null && x.Counterparty is null
                     && x.OneoffName.Length == 0 && x.HasParty))
        {
            var key = line.Edrpou.Length > 0 ? "code:" + line.Edrpou
                : line.Iban.Length > 0 ? "iban:" + line.Iban
                : "name:" + line.BankName.ToLowerInvariant();
            if (!created.TryGetValue(key, out var counterparty))
            {
                var name = line.BankName.Length > 0 ? line.BankName
                    : line.Edrpou.Length > 0 ? $"ЄДРПОУ {line.Edrpou}"
                    : $"IBAN {line.Iban}";
                var details = FinanceCounterpartyDetails.Of(name,
                    line.Edrpou.Length <= FinanceCounterpartyDetails.EdrpouMaxLength ? line.Edrpou : null,
                    line.Iban.Length <= FinanceCounterpartyDetails.IbanMaxLength ? line.Iban : null, line.BankName, null,
                    null, false, $"Створено з виписки «{statement.FileName}».");
                counterparty = FinanceCounterparty.Create(FinanceCounterpartyId.New(), details, null, verified: false, now);
                db.Counterparties.Add(counterparty);
                created[key] = counterparty;
            }

            line.UseCreatedCounterparty(counterparty.State);
        }

        return created.Values.ToDictionary(x => x.Link, x => x.State);
    }
}
