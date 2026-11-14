namespace Accounting.Application.Features.Accounts.Shared;

/// <summary>
/// Balances of one account in kopecks, counted from active movements only. <c>LegacyMinor</c>: opening balance plus
/// «Домашня бухгалтерія»; <c>CurrentMinor</c>: confirmed balance on 01.10.2026 plus the current books, null while that
/// balance is not confirmed; <c>TotalMinor</c>: opening balance plus every movement of both areas (not the sum of the
/// other two). <c>HasHistory</c> is true once any movement, cancelled ones too, uses the account.
/// </summary>
public sealed record FinanceAccountBalanceResponse(long LegacyMinor, long? CurrentMinor, long TotalMinor, bool HasHistory);
