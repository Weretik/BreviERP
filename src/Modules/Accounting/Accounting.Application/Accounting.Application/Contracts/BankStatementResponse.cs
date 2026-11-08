using Accounting.Application.Lookup;
using Accounting.Domain.Categories;
using Accounting.Domain.Statements;

namespace Accounting.Application.Contracts;

/// <summary>
/// An uploaded statement of «Виписки банку»: the account, the file, the range of line dates, the state and, once
/// booked, how many operations and transfers it added. <c>Existing</c> is true when the same file was uploaded before
/// and that statement is returned.
/// </summary>
public sealed record BankStatementResponse(
    Guid Id,
    Guid AccountId,
    string AccountName,
    string FileName,
    BankStatementStatus Status,
    int LineCount,
    DateOnly? FirstDate,
    DateOnly? LastDate,
    int TransactionCount,
    int TransferCount,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    string? BookedBy,
    DateTimeOffset? BookedAt,
    int Version,
    bool Existing = false)
{
    internal static BankStatementResponse From(BankStatement x, string accountName, bool existing = false) =>
        new(x.Id.Value, x.AccountId.Value, accountName, x.FileName, x.Status, x.LineCount, x.FirstDate, x.LastDate,
            x.TransactionCount, x.TransferCount, x.CreatedBy, x.CreatedAt, x.BookedBy, x.BookedAt, x.Version, existing);
}

/// <summary>
/// A statement with a page of its lines (100 per page, in file order, optionally of one <c>State</c>), the number of
/// lines in each state and what «Провести виписку» would add now.
/// </summary>
public sealed record BankStatementDetailsResponse(
    BankStatementResponse Statement,
    IReadOnlyList<BankLineStateCount> States,
    BankStatementTotals Totals,
    IReadOnlyList<BankStatementLineResponse> Lines,
    int Total,
    int Page,
    int PageSize);

public sealed record BankLineStateCount(BankLineState State, int Count);

/// <summary>Lines that would be booked: operations with their income and expense in kopecks, and transfers.</summary>
public sealed record BankStatementTotals(int Operations, long IncomeMinor, long ExpenseMinor, int Transfers);

/// <summary>
/// A statement line: the bank data as read (<c>Error</c> says why a line cannot be booked), what was recognised and what
/// the director chose. <c>TransactionId</c>/<c>TransferId</c> name the record a booked line became.
/// </summary>
public sealed record BankStatementLineResponse(
    Guid Id,
    int RowNumber,
    DateOnly? Date,
    string? Time,
    CategoryDirection? Type,
    long? AmountMinor,
    string? Currency,
    string BankName,
    string Edrpou,
    string Iban,
    string Purpose,
    string Document,
    string? BankId,
    string Error,
    BankDuplicate Duplicate,
    string DuplicateNote,
    Guid? TransferAccountId,
    string? TransferAccountName,
    FinanceCounterpartyRefResponse? Counterparty,
    Guid? CategoryId,
    Guid? CategoryRootId,
    string? CategoryRootName,
    string? SubcategoryName,
    string OneoffName,
    string Description,
    string Comment,
    bool Excluded,
    BankLineState State,
    bool WillBook,
    Guid? TransactionId,
    Guid? TransferId)
{
    internal static BankStatementLineResponse From(BankStatementLine x, FinanceNames names)
    {
        var category = names.Category(x.CategoryId);
        var root = names.Root(category);
        var counterparty = x.Counterparty is { } link
            ? new FinanceCounterpartyRefResponse(link.Kind, link.Id, x.CounterpartyName, true, x.CounterpartyVerified, false)
            : null;
        return new BankStatementLineResponse(x.Id.Value, x.RowNumber, x.Date, x.Time?.ToString("HH:mm:ss"), x.Type,
            x.AmountMinor, x.Currency, x.BankName, x.Edrpou, x.Iban, x.Purpose, x.Document, x.BankId, x.Error, x.Duplicate,
            x.DuplicateNote, x.TransferAccountId?.Value,
            x.TransferAccountId is { } other ? names.Account(other) : null, counterparty, x.CategoryId?.Value,
            root?.Id.Value, root?.Name, category is { IsRoot: false } ? category.Name : null, x.OneoffName, x.Description,
            x.Comment, x.Excluded, x.State, x.WillBook, x.TransactionId?.Value, x.TransferId?.Value);
    }
}
