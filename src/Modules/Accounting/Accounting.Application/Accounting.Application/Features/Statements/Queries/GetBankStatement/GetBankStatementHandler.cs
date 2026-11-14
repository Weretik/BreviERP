using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Movements;
using Accounting.Domain.Statements;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Statements.Shared;
using Accounting.Domain.Statements.Enums;
using Accounting.Domain.Statements.ValueObjects;

namespace Accounting.Application.Features.Statements.Queries.GetBankStatement;

public sealed class GetBankStatementHandler(IAccountingDbContext db)
    : IQueryHandler<GetBankStatementQuery, Result<BankStatementDetailsResponse>>
{
    public const int PageSize = 100;

    public async ValueTask<Result<BankStatementDetailsResponse>> Handle(GetBankStatementQuery query,
        CancellationToken cancellationToken)
    {
        var id = BankStatementId.From(query.Id);
        var statement = await db.BankStatements.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (statement is null) return Result.NotFound(StatementMessages.NotFound);

        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        var lines = db.BankStatementLines.AsNoTracking().Where(x => x.StatementId == id);
        var summary = await lines
            .Select(x => new { x.State, x.Excluded, x.Duplicate, x.Type, x.AmountMinor, x.TransferAccountId })
            .ToListAsync(cancellationToken);
        var states = summary.GroupBy(x => x.State).OrderBy(x => x.Key)
            .Select(x => new BankLineStateCount(x.Key, x.Count())).ToList();
        var booked = summary.Where(x => !x.Excluded && x.State != BankLineState.Error && x.Duplicate != BankDuplicate.Exact)
            .ToList();
        var operations = booked.Where(x => x.TransferAccountId == null).ToList();
        var totals = new BankStatementTotals(operations.Count,
            operations.Where(x => x.Type == CategoryDirection.Income).Sum(x => x.AmountMinor ?? 0),
            operations.Where(x => x.Type == CategoryDirection.Expense).Sum(x => x.AmountMinor ?? 0),
            booked.Count - operations.Count);

        var filtered = query.State is { } state ? lines.Where(x => x.State == state) : lines;
        var total = await filtered.CountAsync(cancellationToken);
        var page = await filtered.OrderBy(x => x.RowNumber).Skip((query.Page - 1) * PageSize).Take(PageSize)
            .ToListAsync(cancellationToken);

        return Result.Success(new BankStatementDetailsResponse(
            BankStatementResponse.From(statement, names.Account(statement.AccountId)), states, totals,
            page.Select(x => BankStatementLineResponse.From(x, names)).ToList(), total, query.Page, PageSize,
            statement.Status == BankStatementStatus.Booked ? await RollbackAsync(id, cancellationToken) : null));
    }

    /// <summary>Booked records start at version 1; a later version means a change or a cancel by hand.</summary>
    private async Task<BankStatementRollback> RollbackAsync(BankStatementId id, CancellationToken cancellationToken)
    {
        var lines = db.BankStatementLines.AsNoTracking().Where(x => x.StatementId == id);
        var transactions = await db.Transactions.AsNoTracking()
            .Where(t => lines.Any(x => x.TransactionId == t.Id))
            .Select(x => new { x.Status, x.Version }).ToListAsync(cancellationToken);
        var transfers = await db.Transfers.AsNoTracking()
            .Where(t => lines.Any(x => x.TransferId == t.Id))
            .Select(x => new { x.Status, x.Version }).ToListAsync(cancellationToken);
        var records = transactions.Concat(transfers).ToList();
        return new BankStatementRollback(
            transactions.Count(x => x.Status == MovementStatus.Active),
            transfers.Count(x => x.Status == MovementStatus.Active),
            records.Count(x => x.Status == MovementStatus.Active && x.Version > 1),
            records.Count(x => x.Status == MovementStatus.Void));
    }
}
