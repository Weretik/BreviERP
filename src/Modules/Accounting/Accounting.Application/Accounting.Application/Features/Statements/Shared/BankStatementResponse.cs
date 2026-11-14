using Accounting.Domain.Categories;
using Accounting.Domain.Statements;

namespace Accounting.Application.Features.Statements.Shared;

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
