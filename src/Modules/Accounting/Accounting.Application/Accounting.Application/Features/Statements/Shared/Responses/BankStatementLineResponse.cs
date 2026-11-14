using Accounting.Domain.Categories;
using Accounting.Domain.Statements;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Domain.Statements.Enums;

namespace Accounting.Application.Features.Statements.Shared.Responses;

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
